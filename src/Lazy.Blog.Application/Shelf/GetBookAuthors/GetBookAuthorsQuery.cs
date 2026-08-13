using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetBookAuthorsQuery(string? Query) : IQuery<IReadOnlyList<BookAuthorResponse>>;

public sealed class GetBookAuthorsQueryHandler(IShelfRepository shelf) : IQueryHandler<GetBookAuthorsQuery, IReadOnlyList<BookAuthorResponse>>
{
    public async Task<Result<IReadOnlyList<BookAuthorResponse>>> Handle(GetBookAuthorsQuery request, CancellationToken ct) =>
        (await shelf.GetAuthorsAsync(request.Query, 10, ct)).Select(x => new BookAuthorResponse(x.Id, x.Name)).ToList();
}
