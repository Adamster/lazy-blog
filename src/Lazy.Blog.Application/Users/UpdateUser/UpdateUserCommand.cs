using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string DisplayName,
    string Username,
    string? Biography = null) : ICommand;
