using Lazy.Application.Abstractions.Messaging;
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

    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId,
            cancellationToken);

        if (post is null)
        {
            return Result.Failure<PostResponse>(new Error(
                "Post.NotFound",
                $"The post with Id {request.PostId} was not found."));
        }

        var response = new PostResponse(
            post.Id,
            post.Title.Value, 
            post.Summary.Value,
            post.Body.Value);

        return response;
    }
}