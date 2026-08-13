using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetSeriesBySlugQuery(string Slug) : IQuery<SeriesDetailResponse>;

public sealed class GetSeriesBySlugQueryHandler(IShelfRepository shelf, ICurrentUserContext currentUser) : IQueryHandler<GetSeriesBySlugQuery, SeriesDetailResponse>
{
    public async Task<Result<SeriesDetailResponse>> Handle(GetSeriesBySlugQuery request, CancellationToken ct)
    {
        var series = await shelf.GetSeriesBySlugAsync(request.Slug, ct);
        if (series is null) return Result.Failure<SeriesDetailResponse>(DomainErrors.Shelf.SeriesSlugNotFound(request.Slug));
        var volumes = await shelf.GetBooksBySeriesIdAsync(series.Id, ct);
        var statuses = await shelf.GetUserBookStatusesAsync(currentUser.GetCurrentUserId(), volumes.Select(book => book.Id).ToList(), ct);
        return new SeriesDetailResponse(series.Id, series.Slug, series.Title,
            volumes.Select(book => ShelfMapping.ToResponse(book, ShelfMapping.LookupStatus(statuses, book.Id))).ToList());
    }
}
