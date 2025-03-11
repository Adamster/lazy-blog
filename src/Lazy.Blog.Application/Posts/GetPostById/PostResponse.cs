using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetPostById;

public record PostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Body,
    string Slug,
    bool IsPublished,
    UserResponse Author,
    IList<TagPostResponse> Tags,
    string? CoverUrl);