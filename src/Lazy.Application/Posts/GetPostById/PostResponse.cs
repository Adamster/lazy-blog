namespace Lazy.Application.Posts.GetPostById;

public record PostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Body,
    string Slug,
    string? CoverUrl);