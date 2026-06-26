using Lazy.Domain.Entities;

namespace Lazy.Application.Posts.Models;

public record AuthorPostResponse(
    Guid Id,
    string DisplayName,
    string UserName,
    string? Biography,
    string? AvatarUrl,
    DateTime CreatedOnUtc)
{
    public static AuthorPostResponse FromPost(Post post)
    {
        return new AuthorPostResponse(
            post.UserId,
            post.User.DisplayName.Value,
            post.User.UserName!,
            post.User.Biography?.Value,
            post.User.Avatar?.Url,
            post.User.CreatedOnUtc);
    }

    public AuthorPostResponse(User user) :
        this(user.Id,
            user.DisplayName.Value,
            user.UserName!,
            user.Biography?.Value,
            user.Avatar?.Url,
            user.CreatedOnUtc)
    {
    }

}
