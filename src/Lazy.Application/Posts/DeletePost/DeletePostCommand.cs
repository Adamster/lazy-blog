using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.DeletePost;

public sealed record DeletePostCommand(Guid PostId) : ICommand;