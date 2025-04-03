using Lazy.Application.Posts.Models;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPostBySlug;

public record PostDetailedResponse(
    Guid Id,
    string Title,
    string? Summary,
    AuthorPostResponse Author,
    string Slug,
    string Body,
    string? CoverUrl,
    bool IsCoverDisplayed,
    IList<TagPostResponse> Tags,
    int Rating,
    long Views,
    bool IsPublished,
    VoteDirection? VoteDirection,
    DateTime CreatedAtUtc);