using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Comments.GetCommentById;

public record GetCommentByIdQuery(Guid CommentId) : IQuery<CommentResponse>;