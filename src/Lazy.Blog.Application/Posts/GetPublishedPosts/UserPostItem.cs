using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPublishedPosts;

public record UserPostItem(
    Guid Id,
    string Title,
    string? Summary,
    string Slug,
    long Views,
    int Comments,
    int Rating,
    VoteDirection? VoteDirection,
    string? CoverUrl,
    bool IsPublished,
    List<TagPostResponse> Tags,
    DateTime CreatedAtUtc);