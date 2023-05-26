using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Application.Posts.UpdatePost;
public record UpdatePostCommand(
    Guid Id,
    string Title,
    string? Summary,
    string Body,
    string Slug,
    string? CoverUrl,
    List<TagResponse> Tags,
    bool IsPublished) : ICommand;