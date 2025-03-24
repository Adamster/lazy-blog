using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.Extensions;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Comments.GetCommentById;

public class GetCommentByIdQueryHandler : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken ct)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, ct);

        if (comment is null)
        {
            return Result.Failure<CommentResponse>(DomainErrors.Comment.NotFound(request.CommentId));
        }

        return new CommentResponse(
            comment.Id,
            comment.User.ToUserCommentResponse(),
            comment.CommentText.Value,
            comment.CreatedOnUtc);
    }
}