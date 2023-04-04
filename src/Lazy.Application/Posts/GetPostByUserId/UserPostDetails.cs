namespace Lazy.Application.Posts.GetPostByUserId;

public record UserPostDetails(
    Guid PostId,
    string Title,
    string Summary,
    string Slug,
    bool IsPublished,
    DateTime CreatedAtUtc);