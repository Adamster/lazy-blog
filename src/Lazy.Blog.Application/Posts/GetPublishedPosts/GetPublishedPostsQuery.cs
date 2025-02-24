using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPostById;

namespace Lazy.Application.Posts.GetPublishedPosts;

public record GetPublishedPostsQuery(int Offset) : IQuery<List<PublishedPostResponse>>;