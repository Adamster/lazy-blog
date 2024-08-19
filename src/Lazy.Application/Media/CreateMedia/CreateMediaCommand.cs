using Lazy.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Media.CreateMedia;

public record CreateMediaCommand(Guid UserId, IFormFile File) : ICommand<string>;