using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Models;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPostBySlug;

public class GetPostBySlugHandler : IQueryHandler<GetPostBySlugQuery, PostDetailedResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IPostVoteRepository _postVoteRepository;

    public GetPostBySlugHandler(IPostRepository postRepository, ICurrentUserContext userContext, IPostVoteRepository postVoteRepository)
    {
        _postRepository = postRepository;
        _userContext = userContext;
        _postVoteRepository = postVoteRepository;
    }

    public async Task<Result<PostDetailedResponse>> Handle(GetPostBySlugQuery request, CancellationToken ct)
    {
        var slugResult = Slug.Create(request.Slug);

        if (slugResult.IsFailure)
        {
            return Result.Failure<PostDetailedResponse>(slugResult.Error);
        }

        Post? post = await _postRepository.GetBySlugAsync(slugResult.Value, ct);

        if (post is null)
        {
            return Result.Failure<PostDetailedResponse>(
                DomainErrors.Post.SlugNotFound(slugResult.Value));
        }

        var currentUserId = _userContext.GetCurrentUserId();

        PostVote? postVote = null;
        if (currentUserId != Guid.Empty)
        {
           postVote =  await _postVoteRepository.GetPostVoteForUserIdAsync(currentUserId, post.Id, ct);
        }

        var postResponse = new PostDetailedResponse
        (
            post.Id,
            post.Title.Value,
            post.Summary?.Value,
            new AuthorPostResponse(post.User),
            post.Slug.Value,
            post.Body.Value,
            post.CoverUrl,
            post.Tags.Select(t => new TagPostResponse(t.Id, t.Value)).ToList(),
            post.Rating,
            post.Views,
            post.IsPublished,
            postVote?.VoteDirection,
            post.CreatedOnUtc
        );

        return postResponse;
    }
}