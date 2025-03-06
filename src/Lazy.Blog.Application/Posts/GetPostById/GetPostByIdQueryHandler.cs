using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostById;

public class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery request, CancellationToken ct)
    {
        Post? post = await _postRepository.GetByIdAsync(request.PostId,
            ct);

        if (post is null)
        {
            return Result.Failure<PostResponse>(new Error(
                "Post.NotFound",
                $"The post with Id {request.PostId} was not found."));
        }

        var response = new PostResponse(
            post.Id,
            post.Title.Value,
            post.Summary?.Value,
            post.Body.Value,
            post.Slug.Value,
            post.IsPublished,
            new UserResponse(post.User),
            post.Tags.Select(t => new TagResponse(t.Id, t.Value)).ToList(),
            post.CoverUrl);

        return response;
    }
}