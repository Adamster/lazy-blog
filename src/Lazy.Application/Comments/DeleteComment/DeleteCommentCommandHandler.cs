using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(
        ICurrentUserContext currentUserContext,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserContext = currentUserContext;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (comment is null)
        {
            return Result.Failure(DomainErrors.Comment.NotFound(request.Id));
        }

        if (!_currentUserContext.IsCurrentUser(comment.UserId))
        {
            return Result.Failure(DomainErrors.Comment.UnauthorizedCommentUpdate);
        }

        _commentRepository.Delete(comment);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         return Result.Success();
    }
}