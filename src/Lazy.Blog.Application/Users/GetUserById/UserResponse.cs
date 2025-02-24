using Lazy.Domain.Entities;

namespace Lazy.Application.Users.GetUserById;

public record UserResponse(Guid Id, string Email, string FirstName, string LastName, string UserName, string? AvatarUrl, DateTime CreatedOnUtc)
{
    public UserResponse(User user) : this(user.Id, user.Email.Value, user.FirstName.Value, user.LastName.Value, user.UserName.Value, user.Avatar?.Url, user.CreatedOnUtc)
    {
    }
}