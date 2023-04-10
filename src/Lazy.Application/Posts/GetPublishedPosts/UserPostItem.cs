namespace Lazy.Application.Posts.GetPublishedPosts;

public record UserPostItem(
    Guid Id,
    string Title,
    string Summary,
    string Slug,
    long Views,
    int Comments,
    string? CoverUrl,
    bool IsPublished,
    DateTime CreatedAtUtc);