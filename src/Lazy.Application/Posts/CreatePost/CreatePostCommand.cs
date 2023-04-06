using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.CreatePost;

public record CreatePostCommand(
    string Title,
    string Summary,
    string Body,
    bool IsPublished,
    Guid UserId) : ICommand<Guid>;