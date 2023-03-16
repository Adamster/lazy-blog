namespace Lazy.Application.Users.GetUserById;

public record UserResponse(Guid Id, string Email, string FirstName, string LastName);