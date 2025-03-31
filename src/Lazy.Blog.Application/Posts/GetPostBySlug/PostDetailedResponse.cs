using Lazy.Application.Posts.Models;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Application.Posts.GetPostBySlug;

public record PostDetailedResponse(
    Guid Id,
    string Title,
    string? Summary,
    AuthorPostResponse Author,
    string Slug,
    string Body,
    string? CoverUrl,
    IList<TagPostResponse> Tags,
    int Rating,
    long Views,
    bool IsPublished,
    DateTime CreatedAtUtc);