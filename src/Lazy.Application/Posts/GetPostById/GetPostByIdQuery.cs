using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.GetPostById;

public record GetPostByIdQuery(Guid PostId) : IQuery<PostResponse>;