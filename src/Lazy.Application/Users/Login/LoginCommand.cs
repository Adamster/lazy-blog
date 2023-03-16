using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.Login;

public record LoginCommand(string Email, string Password) : ICommand<string>;