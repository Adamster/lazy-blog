using Lazy.Domain.Entities;

namespace Lazy.Application.Users.GetUserById;

public record UserResponse(Guid Id, string Email, string FirstName, string LastName)
{
    public UserResponse(User user) : this(user.Id, user.Email.Value, user.FirstName.Value, user.LastName.Value)
    {
    }
}