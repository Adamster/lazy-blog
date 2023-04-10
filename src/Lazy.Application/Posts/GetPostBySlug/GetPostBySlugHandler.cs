using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPostBySlug;

public class GetPostBySlugHandler : IQueryHandler<GetPostBySlugQuery, PostDetailedResponse>
{
    private readonly IPostRepository _postRepository;

    public GetPostBySlugHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PostDetailedResponse>> Handle(GetPostBySlugQuery request, CancellationToken cancellationToken)
    {
        var slugResult = Slug.Create(request.Slug);

        if (slugResult.IsFailure)
        {
            return Result.Failure<PostDetailedResponse>(slugResult.Error);
        }

        Post? post = await _postRepository.GetBySlugAsync(slugResult.Value, cancellationToken);

        if (post is null)
        {
            return Result.Failure<PostDetailedResponse>(
                DomainErrors.Post.SlugNotFound(slugResult.Value));
        }

        var postResponse = new PostDetailedResponse
        (
            post.Id,
            post.Title.Value,
            post.Summary.Value,
            new UserResponse(post.User),
            post.Body.Value,
            post.CoverUrl,
            post.Views,
            post.CreatedOnUtc
        );

        return postResponse;
    }
}