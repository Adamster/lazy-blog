using Lazy.Application.Users.GetUserById;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPublishedPosts;

public record PublishedPostResponse(
    Guid Id,
    string Title,
    string? Summary,
    string Slug,
    UserResponse Author,
    long Views,
    int Comments,
    int Rating,
    VoteDirection? VoteDirection,
    string? CoverUrl,
    DateTime CreatedAtUtc);