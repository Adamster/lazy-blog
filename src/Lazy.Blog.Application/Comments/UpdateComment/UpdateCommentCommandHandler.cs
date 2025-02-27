using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Comments.UpdateComment;

public class UpdateCommentCommandHandler(
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext)
    : ICommandHandler<UpdateCommentCommand>
{
    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.UserId));
        }

        var bodyUpdateResult = Body.Create(request.Body);

        if (!currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure(DomainErrors.Comment.UnauthorizedCommentUpdate);
        }

        Comment? comment = await commentRepository.GetByIdAsync(request.CommentId, ct);
        if (comment is null)
        {
            return Result.Failure(DomainErrors.Comment.NotFound(request.CommentId));
        }

        comment.Update(bodyUpdateResult.Value);
        
        commentRepository.Update(comment);

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}