using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.ResetPassword;

public record ResetPasswordCommand(string Token, string Email, string NewPassword) : ICommand<bool>;
