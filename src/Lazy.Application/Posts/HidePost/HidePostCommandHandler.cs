using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.HidePost;

public class HidePostCommandHandler : ICommandHandler<HidePostCommand>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HidePostCommandHandler(ICurrentUserContext currentUserContext, IPostRepository postRepository, IUnitOfWork unitOfWork)
    {
        _currentUserContext = currentUserContext;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(HidePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);

        if (post is null)
        {
            return Result.Failure(DomainErrors.Post.NotFound(request.Id));
        }

        if (!_currentUserContext.IsCurrentUser(post.UserId))
        {
            return Result.Failure(DomainErrors.Post.UnauthorizedPostAccess);
        }

        post.Hide();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}