using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Users.Login;

public record LoginResponse(string AccessToken, string RefreshToken, UserResponse User);