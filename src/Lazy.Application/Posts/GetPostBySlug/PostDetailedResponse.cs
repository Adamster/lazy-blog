using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetPostBySlug;

public record PostDetailedResponse(
    string Title,
    string Summary,
    UserResponse Author,
    string Body,
    DateTime CreatedAtUtc);