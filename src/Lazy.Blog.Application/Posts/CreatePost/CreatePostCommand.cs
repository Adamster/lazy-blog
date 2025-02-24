using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Application.Posts.CreatePost;

public record CreatePostCommand(
    string Title,
    string? Summary,
    string Body,
    bool IsPublished,
    List<TagResponse>? Tags,
    string? CoverUrl,
    Guid UserId) : ICommand<PostCreatedResponse>;