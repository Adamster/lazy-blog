using Lazy.Application.Tags.SearchTag;

namespace Lazy.Presentation.Contracts.Posts;

public record UpdatePostRequest(
    string Title,
    string Summary,
    string Body,
    string Slug,
    string? CoverUrl,
    List<TagResponse> Tags,
    bool IsPublished);