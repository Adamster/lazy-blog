using Lazy.Application.Tags.SearchTag;

namespace Lazy.Application.Posts.GetPostById;

public record PostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Body,
    string Slug,
    IList<TagResponse> Tags,
    string? CoverUrl);