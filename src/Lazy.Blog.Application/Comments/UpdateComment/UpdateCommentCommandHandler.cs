using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Comments.UpdateComment;

public class UpdateCommentCommandHandler : ICommandHandler<UpdateCommentCommand>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork, ICurrentUserContext currentUserContext)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.UserId));
        }

        var bodyUpdateResult = Body.Create(request.Body);

        if (!_currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure(DomainErrors.Comment.UnauthorizedCommentUpdate);
        }

        Comment? comment = await _commentRepository.GetByIdAsync(request.CommentId, ct);
        if (comment is null)
        {
            return Result.Failure(DomainErrors.Comment.NotFound(request.CommentId));
        }

        comment.Update(bodyUpdateResult.Value);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}