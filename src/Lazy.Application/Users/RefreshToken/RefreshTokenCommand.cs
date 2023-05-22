using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.Login;

namespace Lazy.Application.Users.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<LoginResponse>;