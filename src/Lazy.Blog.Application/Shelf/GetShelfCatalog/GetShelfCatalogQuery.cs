using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetShelfCatalogQuery(
    string? Genre,
    string? AuthorSlug,
    string? Search,
    string? Sort,
    int Offset,
    int Limit) : IQuery<ShelfCatalogResponse>;

public sealed class GetShelfCatalogQueryValidator : AbstractValidator<GetShelfCatalogQuery>
{
    public GetShelfCatalogQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 500);
    }
}

public sealed class GetShelfCatalogQueryHandler(
    IBookRepository books,
    IUserBookStatusRepository userBookStatuses,
    IRatingSyncRunRepository ratingSyncRuns,
    ICurrentUserContext currentUser) : IQueryHandler<GetShelfCatalogQuery, ShelfCatalogResponse>
{
    public async Task<Result<ShelfCatalogResponse>> Handle(GetShelfCatalogQuery request, CancellationToken ct)
    {
        int offset = Math.Max(0, request.Offset);
        int limit = Math.Clamp(request.Limit, 1, 500);

        var filtered = await books.GetFilteredAsync(request.Genre, request.AuthorSlug, request.Search, ct);
        var standalone = filtered.Where(book => book.SeriesId is null).ToList();
        var matchingSeriesIds = filtered
            .Where(book => book.SeriesId is not null)
            .Select(book => book.SeriesId!.Value)
            .Distinct()
            .ToList();
        var seriesGroups = matchingSeriesIds.Count == 0
            ? []
            : (await books.GetBySeriesIdsAsync(matchingSeriesIds, ct))
                .GroupBy(book => book.SeriesId!.Value)
                .ToList();

        var allBookIds = standalone.Select(book => book.Id)
            .Concat(seriesGroups.SelectMany(group => group.Select(book => book.Id)))
            .ToList();
        var rawStatuses = await userBookStatuses.GetStatusesAsync(currentUser.GetCurrentUserId(), allBookIds, ct);
        var statuses = allBookIds.ToDictionary(
            id => id,
            id => new BookStatusInfo(rawStatuses.TryGetValue(id, out var status) ? status : null, null));

        var (items, totalItems) = ShelfCardBuilder.Build(standalone, seriesGroups, statuses, request.Sort, offset, limit);
        int totalBooks = standalone.Count + seriesGroups.Sum(group => group.Count());

        return new ShelfCatalogResponse(items, totalItems, totalBooks, offset, limit, await ratingSyncRuns.GetLastSyncAsync(ct));
    }
}
