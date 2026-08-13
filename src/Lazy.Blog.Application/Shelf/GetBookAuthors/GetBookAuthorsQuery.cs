using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetBookAuthorsQuery(string? Query) : IQuery<IReadOnlyList<BookAuthorResponse>>;

public sealed class GetBookAuthorsQueryHandler(
    IAuthorRepository authors) : IQueryHandler<GetBookAuthorsQuery, IReadOnlyList<BookAuthorResponse>>
{
    public async Task<Result<IReadOnlyList<BookAuthorResponse>>> Handle(
        GetBookAuthorsQuery request,
        CancellationToken ct)
    {
        return (await authors.GetAllAsync(request.Query, 10, ct))
            .Select(author => new BookAuthorResponse(author.Id, author.Slug, author.Name, author.NameEn))
            .ToList();
    }
}
