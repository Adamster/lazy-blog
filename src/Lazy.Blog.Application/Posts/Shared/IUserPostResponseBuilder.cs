using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Domain.Entities;

namespace Lazy.Application.Posts.Shared;

public interface IUserPostResponseBuilder
{
    Task<UserPostResponse> BuildAsync(User user, IQueryable<Post> posts, CancellationToken ct);
}
