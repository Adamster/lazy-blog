using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.CreatePost;

public record CreatePostCommand(
    string Title,
    string Summary,
    string Body,
    Guid UserId) : ICommand<Guid>;