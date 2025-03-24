using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;