using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.HidePost;

public record HidePostCommand(Guid Id) : ICommand;