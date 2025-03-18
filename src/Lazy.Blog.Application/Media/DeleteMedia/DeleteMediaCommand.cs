using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Media.DeleteMedia;

public record DeleteMediaCommand(string BlobUrl) : ICommand;