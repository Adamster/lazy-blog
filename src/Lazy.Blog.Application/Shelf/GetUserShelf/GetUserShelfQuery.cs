using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetUserShelfQuery(
    string UserName,
    string? Status,
    string? Genre,
    string? Search,
    string? Sort,
    int Offset,
    int Limit) : IQuery<ShelfCatalogResponse>;

public sealed class GetUserShelfQueryValidator : AbstractValidator<GetUserShelfQuery>
{
    public GetUserShelfQueryValidator()
    {
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Status).Must(IsValidStatus);
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 500);
    }

    private static bool IsValidStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase) ||
        ShelfMapping.TryStatus(status, out _);
}

public sealed class GetUserShelfQueryHandler(
    IBookRepository books,
    IUserBookStatusRepository userBookStatuses,
    IRatingSyncRunRepository ratingSyncRuns) : IQueryHandler<GetUserShelfQuery, ShelfCatalogResponse>
{
    public async Task<Result<ShelfCatalogResponse>> Handle(GetUserShelfQuery request, CancellationToken ct)
    {
        int offset = Math.Max(0, request.Offset);
        int limit = Math.Clamp(request.Limit, 1, 500);
        ReadingStatus? statusFilter = ParseStatusFilter(request.Status);

        var entries = await userBookStatuses.GetFilteredAsync(request.UserName, statusFilter, request.Genre, request.Search, ct);
        var standalone = entries.Where(entry => entry.Book.SeriesId is null).Select(entry => entry.Book).ToList();
        var matchingSeriesIds = entries
            .Where(entry => entry.Book.SeriesId is not null)
            .Select(entry => entry.Book.SeriesId!.Value)
            .Distinct()
            .ToList();
        var seriesGroups = matchingSeriesIds.Count == 0
            ? []
            : (await books.GetBySeriesIdsAsync(matchingSeriesIds, ct))
                .GroupBy(book => book.SeriesId!.Value)
                .ToList();

        var seriesBookIds = seriesGroups.SelectMany(group => group.Select(book => book.Id)).ToList();
        var seriesEntries = seriesBookIds.Count == 0
            ? new Dictionary<Guid, UserBookStatus>()
            : await userBookStatuses.GetEntriesAsync(request.UserName, seriesBookIds, ct);

        var statuses = new Dictionary<Guid, BookStatusInfo>();
        foreach (var entry in entries)
        {
            statuses[entry.BookId] = new BookStatusInfo(entry.Status, entry.UpdatedOnUtc ?? entry.CreatedOnUtc);
        }
        foreach (var (bookId, entry) in seriesEntries)
        {
            statuses[bookId] = new BookStatusInfo(entry.Status, entry.UpdatedOnUtc ?? entry.CreatedOnUtc);
        }

        var (items, totalItems) = ShelfCardBuilder.Build(standalone, seriesGroups, statuses, request.Sort, offset, limit);
        int totalBooks = standalone.Count + seriesGroups.Sum(group => group.Count());

        return new ShelfCatalogResponse(items, totalItems, totalBooks, offset, limit, await ratingSyncRuns.GetLastSyncAsync(ct));
    }

    private static ReadingStatus? ParseStatusFilter(string? status) =>
        !string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase) &&
        ShelfMapping.TryStatus(status, out var parsed)
            ? parsed
            : null;
}
