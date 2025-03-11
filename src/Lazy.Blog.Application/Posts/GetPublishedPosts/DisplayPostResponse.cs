using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPublishedPosts;

public record DisplayPostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Slug,
    bool IsPublished,
    UserResponse Author,
    long Views,
    int Comments,
    int Rating,
    VoteDirection? VoteDirection,
    string? CoverUrl,
    IList<TagPostResponse> Tags,
    DateTime CreatedAtUtc);