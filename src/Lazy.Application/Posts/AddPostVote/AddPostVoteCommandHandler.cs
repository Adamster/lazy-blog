﻿using Lazy.Application.Abstractions.Authorization;
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

    public async Task<Result> Handle(AddPostVoteCommand request, CancellationToken ct)
    {
        Guid currentUserId = _currentUserContext.GetCurrentUserId();
        
        PostVote? postVote = await _postVoteRepository
            .GetPostVoteForUserIdAsync(
                currentUserId,
                request.PostId, 
                ct);

        if (postVote is not null)
        {
            if (postVote.VoteDirection != request.Direction)
            {
                postVote.Post.Vote(request.Direction);
                _postRepository.Update(postVote.Post);
                _postVoteRepository.Delete(postVote);

                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success();
            }

            bool successUpdate = postVote.Update(request.Direction);
            if (!successUpdate)
            {
                return Result.Failure(DomainErrors.Post.PostAlreadyVoted);
            }

            _postVoteRepository.Update(postVote);
        }
        else
        {
            var post = await _postRepository.GetByIdAsync(request.PostId, ct);

            if (post is null)
            {
                return Result.Failure(DomainErrors.Post.NotFound(request.PostId));
            }

            User? user = await _userRepository.GetByIdAsync(currentUserId, ct);

            if (user is null)
            {
                return Result.Failure(DomainErrors.User.NotFound(currentUserId));
            }

            PostVote newPostVote = PostVote.Create(post, user, request.Direction);
            _postVoteRepository.Add(newPostVote);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();

    }
}