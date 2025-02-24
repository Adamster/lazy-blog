using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.GetPostBySlug;

public record GetPostBySlugQuery(string Slug) : IQuery<PostDetailedResponse>;