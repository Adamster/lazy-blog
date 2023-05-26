﻿using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Extensions;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.CreatePost;

internal sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, PostCreatedResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreatePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Result<PostCreatedResponse>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        Result<Title> titleResult = Title.Create(request.Title);
        Result<Summary> summaryResult = Summary.Create(request.Summary);
        Result<Body> bodyResult = Body.Create(request.Body);
        List<Tag> tags = CreateTags(request.Tags);

        if (await _userRepository.GetByIdAsync(request.UserId, cancellationToken) is null)
        {
            return Result.Failure<PostCreatedResponse>(DomainErrors.User.NotFound(request.UserId));
        }

        Result<Slug> slugResult = Slug.Create(request.Title.Slugify());

        Guid postId = Guid.NewGuid();

        if (await _postRepository.GetBySlugAsync(slugResult.Value, cancellationToken) is not null)
        {
            slugResult = Slug.Create($"{postId.ToByteArray().GetHashCode()}-{slugResult.Value.Value}");
        }

        Post post = Post.Create(
            postId,
            titleResult.Value,
            summaryResult.Value,
            slugResult.Value,
            bodyResult.Value,
            request.UserId,
            request.IsPublished,
            tags,
            request.CoverUrl);

        _postRepository.Add(post);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PostCreatedResponse(post.Id, post.Slug.Value);
    }

    private List<Tag> CreateTags(List<string> requestTags)
    {
        var tags = new List<Tag>();
        if (!requestTags.Any())
        {
            return tags;
        }
       
        foreach (var requestTag in requestTags)
        {
            var tagResult = Tag.Create(requestTag);
           tags.Add(tagResult.Value);
        }

        return tags;
    }
}