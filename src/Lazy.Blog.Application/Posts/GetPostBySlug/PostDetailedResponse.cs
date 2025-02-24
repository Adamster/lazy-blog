using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetPostBySlug;

public record PostDetailedResponse(
    Guid Id,
    string Title,
    string? Summary,
    UserResponse Author,
    string Slug,
    string Body,
    string? CoverUrl,
    IList<TagResponse> Tags,
    int Rating,
    long Views,
    DateTime CreatedAtUtc);