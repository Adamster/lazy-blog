using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.ChangePassword;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : ICommand;