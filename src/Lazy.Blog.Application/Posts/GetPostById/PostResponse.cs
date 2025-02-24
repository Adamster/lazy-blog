using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetPostById;

public record PostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Body,
    string Slug,
    UserResponse Author,
    IList<TagResponse> Tags,
    string? CoverUrl);