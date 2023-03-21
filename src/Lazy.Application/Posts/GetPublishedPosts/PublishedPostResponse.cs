using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetPublishedPosts;

public record PublishedPostResponse(
    Guid Id, 
    string Title,
    string Summary,
    string Body,
    UserResponse Author,
    DateTime CreatedAtUtc);