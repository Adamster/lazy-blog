using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetSeriesQuery : IQuery<IReadOnlyList<SeriesResponse>>;

public sealed class GetSeriesQueryHandler(
    ISeriesRepository series,
    IBookRepository books) : IQueryHandler<GetSeriesQuery, IReadOnlyList<SeriesResponse>>
{
    public async Task<Result<IReadOnlyList<SeriesResponse>>> Handle(GetSeriesQuery request, CancellationToken ct)
    {
        var all = await series.GetAllAsync(ct);
        var counts = await books.GetCountsBySeriesAsync(ct);
        return all
            .Select(value => new SeriesResponse(
                value.Id,
                value.Slug,
                value.Title,
                counts.GetValueOrDefault(value.Id),
                value.TitleEn))
            .ToList();
    }
}
