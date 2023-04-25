using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.PublishPost;

public record PublishPostCommand(Guid Id) : ICommand;