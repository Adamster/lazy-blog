using Lazy.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Users.UploadUserAvatar;

public record UploadUserAvatarCommand(Guid UserId, IFormFile File) : ICommand;