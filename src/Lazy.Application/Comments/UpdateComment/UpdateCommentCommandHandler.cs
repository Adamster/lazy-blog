using System.Security.Claims;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;
using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Comments.UpdateComment;

public class UpdateCommentCommandHandler : ICommandHandler<UpdateCommentCommand>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository, 
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.UserId));
        }

        var bodyUpdateResult = Body.Create(request.Body);

        Claim currentUserClaim = _httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier);
      
        if (currentUserClaim.Value != request.UserId.ToString())
        {
            return Result.Failure(DomainErrors.Comment.UnauthorizedCommentUpdate);
        }

        Comment? comment = await _commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure(DomainErrors.Comment.NotFound(request.CommentId));
        }

        comment.Update(bodyUpdateResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}