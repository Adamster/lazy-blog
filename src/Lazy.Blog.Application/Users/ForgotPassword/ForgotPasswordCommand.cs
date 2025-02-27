using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;