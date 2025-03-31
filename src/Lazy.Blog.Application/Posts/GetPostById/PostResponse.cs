using Lazy.Application.Posts.Models;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Application.Posts.GetPostById;

public record PostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Body,
    string Slug,
    bool IsPublished,
    AuthorPostResponse Author,
    IList<TagPostResponse> Tags,
    string? CoverUrl);