using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetSeriesBySlugQuery(string Slug) : IQuery<SeriesDetailResponse>;

public sealed class GetSeriesBySlugQueryHandler(
    ISeriesRepository series,
    IBookRepository books,
    IUserBookStatusRepository userBookStatuses,
    ICurrentUserContext currentUser) : IQueryHandler<GetSeriesBySlugQuery, SeriesDetailResponse>
{
    public async Task<Result<SeriesDetailResponse>> Handle(GetSeriesBySlugQuery request, CancellationToken ct)
    {
        var value = await series.GetBySlugAsync(request.Slug, ct);
        if (value is null)
        {
            return Result.Failure<SeriesDetailResponse>(DomainErrors.Shelf.SeriesSlugNotFound(request.Slug));
        }
        var volumes = await books.GetBySeriesIdAsync(value.Id, ct);
        var statuses = await userBookStatuses.GetStatusesAsync(
            currentUser.GetCurrentUserId(),
            volumes.Select(book => book.Id).ToList(),
            ct);
        var items = volumes
            .Select(book => ShelfMapping.ToResponse(book, ShelfMapping.LookupStatus(statuses, book.Id)))
            .ToList();
        return new SeriesDetailResponse(value.Id, value.Slug, value.Title, items);
    }
}
