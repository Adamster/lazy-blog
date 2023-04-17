using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.GetUserById;
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

    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);

        if (comment is null)
        {
            return Result.Failure<CommentResponse>(DomainErrors.Comment.NotFound(request.CommentId));
        }

        return new CommentResponse(comment.Id, new UserResponse(comment.User),
            "https://metro.co.uk/wp-content/uploads/2015/06/ad_174020392-e1487698550420.jpg", //temp solution :D
            comment.CommentText.Value,
            comment.CreatedOnUtc);
    }
}