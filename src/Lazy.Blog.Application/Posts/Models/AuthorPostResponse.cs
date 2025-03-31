using Lazy.Domain.Entities;

namespace Lazy.Application.Posts.Models;

public record AuthorPostResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string UserName,
    string? Biography,
    string? AvatarUrl,
    DateTime CreatedOnUtc)
{
    public static AuthorPostResponse FromPost(Post post)
    {
        return new AuthorPostResponse(
            post.UserId,
            post.User.FirstName.Value,
            post.User.LastName.Value,
            post.User.UserName!,
            post.User.Biography?.Value,
            post.User.Avatar?.Url,
            post.User.CreatedOnUtc);
    }

}