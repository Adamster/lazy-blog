using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Comments.GetCommentById;

namespace Lazy.Application.Comments.GetCommentByPostSlug;

public record GetCommentByPostIdQuery(Guid PostId) : IQuery<List<CommentResponse>>;
