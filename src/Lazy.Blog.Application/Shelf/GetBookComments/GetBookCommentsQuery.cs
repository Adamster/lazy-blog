using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetBookCommentsQuery(Guid BookId) : IQuery<IReadOnlyList<BookCommentResponse>>;

public sealed class GetBookCommentsQueryHandler(IShelfRepository shelf) : IQueryHandler<GetBookCommentsQuery, IReadOnlyList<BookCommentResponse>>
{
    public async Task<Result<IReadOnlyList<BookCommentResponse>>> Handle(GetBookCommentsQuery request, CancellationToken ct)
    {
        if (!await shelf.BookExistsAsync(request.BookId, ct))
            return Result.Failure<IReadOnlyList<BookCommentResponse>>(DomainErrors.Shelf.BookNotFound(request.BookId));
        return (await shelf.GetBookCommentsAsync(request.BookId, ct)).Select(x => new BookCommentResponse(x.Id,
            new(x.UserId, x.User.DisplayName.Value, x.User.UserName!, x.User.Avatar?.Url), x.Body, x.CreatedOnUtc, x.UpdatedOnUtc)).ToList();
    }
}
