using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.CreateUser;

public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password) : ICommand<Guid>;