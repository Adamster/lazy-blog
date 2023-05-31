using Lazy.Application.Tags.SearchTag;

namespace Lazy.Presentation.Contracts.Posts;

public record CreatePostRequest(
    string Title,
    string Summary,
    string Body,
    Guid UserId,
    List<TagResponse>? Tags,
    string? CoverUrl,
    bool IsPublished = true);