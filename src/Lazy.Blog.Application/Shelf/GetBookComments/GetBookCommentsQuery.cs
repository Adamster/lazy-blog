using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetBookCommentsQuery(Guid BookId) : IQuery<IReadOnlyList<BookCommentResponse>>;

public sealed class GetBookCommentsQueryHandler(
    IBookRepository books,
    IBookCommentRepository comments) : IQueryHandler<GetBookCommentsQuery, IReadOnlyList<BookCommentResponse>>
{
    public async Task<Result<IReadOnlyList<BookCommentResponse>>> Handle(
        GetBookCommentsQuery request,
        CancellationToken ct)
    {
        if (!await books.ExistsAsync(request.BookId, ct))
            return Result.Failure<IReadOnlyList<BookCommentResponse>>(DomainErrors.Shelf.BookNotFound(request.BookId));
        return (await comments.GetByBookIdAsync(request.BookId, ct))
            .Select(comment => new BookCommentResponse(
                comment.Id,
                new(
                    comment.UserId,
                    comment.User.DisplayName.Value,
                    comment.User.UserName!,
                    comment.User.Avatar?.Url),
                comment.Body,
                comment.CreatedOnUtc,
                comment.UpdatedOnUtc))
            .ToList();
    }
}
