using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.CreatePost;

public record CreatePostCommand(
    string Title,
    string? Summary,
    string Body,
    bool IsPublished,
    //TODO: change to tag response
    List<string> Tags,
    string? CoverUrl,
    Guid UserId) : ICommand<PostCreatedResponse>;