using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.Logout;

public record LogoutCommand(string RefreshToken) : ICommand;
