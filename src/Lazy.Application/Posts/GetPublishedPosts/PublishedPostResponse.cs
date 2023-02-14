namespace Lazy.Application.Posts.GetPublishedPosts;

public record PublishedPostResponse(
    Guid Id, 
    string Title,
    string Summary,
    string Body,
    DateTime CreateAtUtc);