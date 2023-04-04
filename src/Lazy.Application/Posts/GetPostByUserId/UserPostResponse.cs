namespace Lazy.Application.Posts.GetPostByUserId;

public record UserPostResponse(
    Guid PostId,
    string Title,
    string Summary,
    string Slug,
    bool IsPublished,
    DateTime CreatedAtUtc);