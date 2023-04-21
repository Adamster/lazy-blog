using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.AddPostVote;

public class AddPostVoteCommandHandler : ICommandHandler<AddPostVoteCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostVoteRepository _postVoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserRepository _userRepository;

    public AddPostVoteCommandHandler(
        IPostRepository postRepository, 
        IUnitOfWork unitOfWork,
        IPostVoteRepository postVoteRepository,
        ICurrentUserContext currentUserContext,
        IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _postVoteRepository = postVoteRepository;
        _currentUserContext = currentUserContext;
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(AddPostVoteCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure(DomainErrors.Post.NotFound(request.PostId));
        }

        Guid currentUserId = _currentUserContext.GetCurrentUserId();
        User? user = await _userRepository.GetByIdAsync(currentUserId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure(DomainErrors.User.NotFound(currentUserId));
        }
        
        PostVote? postVote = await _postVoteRepository
            .GetPostVoteForUserIdAsync(
                currentUserId,
                request.PostId, 
                cancellationToken);

        if (postVote is not null)
        {
            bool successUpdate = postVote.Update(request.Direction);
            if (!successUpdate)
            {
                return Result.Failure(DomainErrors.Post.PostAlreadyVoted);
            }

            _postVoteRepository.Update(postVote);
        }
        else
        {
            PostVote newPostVote = PostVote.Create(post, user, request.Direction);
            _postVoteRepository.Add(newPostVote);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();

    }
}