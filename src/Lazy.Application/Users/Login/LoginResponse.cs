using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Users.Login;

public record LoginResponse(string Token, string RefreshToken, UserResponse User);