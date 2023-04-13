using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Posts.DeletePost;

public class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostCommandHandler(
        IPostRepository postRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure(DomainErrors.Post.NotFound(request.PostId));
        }

        if (!_currentUserContext.IsCurrentUser(post.UserId))
        {
            return Result.Failure(DomainErrors.Post.UnauthorizedPostAccess);
        }

        _postRepository.Delete(post);
       await _unitOfWork.SaveChangesAsync(cancellationToken);

       return Result.Success();
    }
}