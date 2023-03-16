using Lazy.Application.Abstractions.Messaging;
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

        var author = $"{post.User.FirstName} {post.User.LastName}";
        var postResponse = new PostDetailedResponse
        (
            post.Title.Value,
            post.Summary.Value,
            author,
            post.Body.Value,
            post.CreatedOnUtc
        );

        return postResponse;
    }
}