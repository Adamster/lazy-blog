using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Extensions;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPublishedPosts;

public class GetPublishedPostsQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPublishedPostsQuery, List<DisplayPostResponse>>
{
    public Task<Result<List<DisplayPostResponse>>> Handle(GetPublishedPostsQuery request, CancellationToken ct)
    {
        IQueryable<Post> postsOptimized =  postRepository.GetPagedPosts(request.Offset, ct);
        List<DisplayPostResponse> response = postsOptimized.ToListDisplayPostResponse();

        return Task.FromResult<Result<List<DisplayPostResponse>>>(response);
    }
}