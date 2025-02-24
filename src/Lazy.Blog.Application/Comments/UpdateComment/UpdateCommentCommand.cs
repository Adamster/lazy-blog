using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Comments.UpdateComment;

public record UpdateCommentCommand(Guid UserId, Guid CommentId, string Body) : ICommand;