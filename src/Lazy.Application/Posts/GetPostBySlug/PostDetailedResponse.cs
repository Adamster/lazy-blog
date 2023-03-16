namespace Lazy.Application.Posts.GetPostBySlug;

public record PostDetailedResponse(
    string Title,
    string Summary,
    string Author,
    string Body,
    DateTime CreatedAtUtc);