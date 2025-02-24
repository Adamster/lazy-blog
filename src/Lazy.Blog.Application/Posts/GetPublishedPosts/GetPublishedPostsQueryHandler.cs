using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPublishedPosts;

public class GetPublishedPostsQueryHandler : IQueryHandler<GetPublishedPostsQuery, List<PublishedPostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetPublishedPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<List<PublishedPostResponse>>> Handle(GetPublishedPostsQuery request, CancellationToken ct)
    {

        IQueryable<Post> postsOptimized =  _postRepository.GetIQueryablePostsAsync(request.Offset, ct);

        List<PublishedPostResponse> response = postsOptimized
            .Select(p =>
                new PublishedPostResponse(
                    p.Id,
                    p.Title.Value,
                    p.Summary.Value,
                    p.Slug.Value,
                    new UserResponse(p.User),
                    p.Views,
                    p.Comments.Count(),
                    p.Rating,
                    p.User.PostVotes
                        .Where(v => v.PostId== p.Id)
                        .Select(v => v.VoteDirection)
                        .FirstOrDefault(),
                    p.CoverUrl,
                    p.Tags.Select(x => new TagResponse(x.Id, x.Value)).ToList(),
                    p.CreatedOnUtc)).ToList();

        return response;
    }
}