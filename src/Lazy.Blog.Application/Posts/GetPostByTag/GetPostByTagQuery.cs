using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPublishedPosts;

namespace Lazy.Application.Posts.GetPostByTag;

public record GetPostByTagQuery(string Tag) : IQuery<List<DisplayPostResponse>>;