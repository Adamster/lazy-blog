using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetSeriesQuery : IQuery<IReadOnlyList<SeriesResponse>>;

public sealed class GetSeriesQueryHandler(IShelfRepository shelf) : IQueryHandler<GetSeriesQuery, IReadOnlyList<SeriesResponse>>
{
    public async Task<Result<IReadOnlyList<SeriesResponse>>> Handle(GetSeriesQuery request, CancellationToken ct)
    {
        var series = await shelf.GetSeriesAsync(ct);
        var counts = await shelf.GetBookCountsBySeriesAsync(ct);
        return series.Select(value => new SeriesResponse(value.Id, value.Slug, value.Title, counts.GetValueOrDefault(value.Id))).ToList();
    }
}
