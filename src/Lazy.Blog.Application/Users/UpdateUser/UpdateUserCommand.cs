using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string FirstName, 
    string LastName, 
    string Username, 
    string? Biography = null) : ICommand;