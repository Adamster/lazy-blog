using Lazy.Domain.Entities;

namespace Lazy.Application.Users.GetUserById;

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string UserName,
    string? Biography,
    string? AvatarUrl,
    DateTime CreatedOnUtc)
{
    public static UserResponse FromPost(Post post)
    {
        return new UserResponse(
            post.UserId,
            post.User.Email!,
            post.User.FirstName.Value,
            post.User.LastName.Value,
            post.User.UserName!,
            post.User.Biography?.Value,
            post.User.Avatar?.Url,
            post.User.CreatedOnUtc);
    }


    public UserResponse(User user) :
        this(user.Id,
            user.Email!,
            user.FirstName.Value,
            user.LastName.Value,
            user.UserName!,
            user.Biography?.Value,
            user.Avatar?.Url,
            user.CreatedOnUtc)
    {
    }
}    
