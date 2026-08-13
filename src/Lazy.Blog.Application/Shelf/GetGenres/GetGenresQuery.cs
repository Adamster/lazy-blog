using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetGenresQuery : IQuery<IReadOnlyList<GenreResponse>>;

public sealed class GetGenresQueryHandler(
    IGenreRepository genres,
    IBookRepository books) : IQueryHandler<GetGenresQuery, IReadOnlyList<GenreResponse>>
{
    public async Task<Result<IReadOnlyList<GenreResponse>>> Handle(GetGenresQuery request, CancellationToken ct)
    {
        var all = await genres.GetAllAsync(ct);
        var counts = await books.GetCountsByGenreAsync(ct);
        return all
            .Select(genre => new GenreResponse(
                genre.Id,
                genre.Slug,
                genre.Name,
                genre.SortOrder,
                counts.GetValueOrDefault(genre.Id)))
            .ToList();
    }
}
