using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.AddPostView;

public record AddPostViewCommand(Guid Id) : ICommand;