using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.CreatePost;

public record PostCreatedResponse(Guid Id, string Slug);