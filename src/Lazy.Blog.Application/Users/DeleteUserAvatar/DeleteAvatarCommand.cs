using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.DeleteUserAvatar;

public record DeleteAvatarCommand(Guid UserId) : ICommand;