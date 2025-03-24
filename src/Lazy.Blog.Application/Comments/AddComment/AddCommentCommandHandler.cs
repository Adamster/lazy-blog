using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Comments.AddComment;

public class AddCommentCommandHandler(
    IUserRepository userRepository,
    IPostRepository postRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork,
    ICommentRepository commentRepository)
    : ICommandHandler<AddCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure<Guid>(DomainErrors.User.NotFound(request.UserId));
        }

        if (!currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure<Guid>(DomainErrors.Comment.UnauthorizedCommentUpdate);
        }

        var post = await postRepository.GetByIdAsync(request.PostId, ct);
        if (post is null)
        {
            return Result.Failure<Guid>(DomainErrors.Post.NotFound(request.PostId));
        }

        Result<Body> commentBody = Body.Create(request.Body);

        var comment = Comment.Create(post, user, commentBody.Value);
        commentRepository.Add(comment);

        await unitOfWork.SaveChangesAsync(ct);

        return comment.Id;
    }
}