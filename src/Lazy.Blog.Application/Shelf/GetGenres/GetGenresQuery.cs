using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetGenresQuery : IQuery<IReadOnlyList<GenreResponse>>;

public sealed class GetGenresQueryHandler(IShelfRepository shelf) : IQueryHandler<GetGenresQuery, IReadOnlyList<GenreResponse>>
{
    public async Task<Result<IReadOnlyList<GenreResponse>>> Handle(GetGenresQuery request, CancellationToken ct)
    {
        var genres = await shelf.GetGenresAsync(ct);
        var counts = await shelf.GetBookCountsByGenreAsync(ct);
        return genres.Select(x => new GenreResponse(x.Id, x.Slug, x.Name, x.SortOrder, counts.GetValueOrDefault(x.Id))).ToList();
    }
}
