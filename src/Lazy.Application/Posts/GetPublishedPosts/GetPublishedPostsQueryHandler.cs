using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.GetUserById;
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

    public async Task<Result<List<PublishedPostResponse>>> Handle(GetPublishedPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetPostsAsync(request.Offset, cancellationToken);

        List<PublishedPostResponse> response = posts
            .Select(p =>
                new PublishedPostResponse(
                    p.Id,
                    p.Title.Value,
                    p.Summary?.Value,
                    p.Slug.Value,
                    new UserResponse(p.UserId, p.User.Email.Value, p.User.FirstName.Value, p.User.LastName.Value,
                        p.User.UserName.Value),
                    p.Views,
                    p.Comments.Count,
                    p.Rating,
                    p.User.PostVotes.FirstOrDefault(u => u.PostId == p.Id)?.VoteDirection,
                    p.CoverUrl,
                    p.CreatedOnUtc))
            .ToList();

        return response;
    }
}