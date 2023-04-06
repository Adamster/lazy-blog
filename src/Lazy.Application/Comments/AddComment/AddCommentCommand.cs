using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Comments.AddComment;

public record AddCommentCommand(Guid PostId, Guid UserId, string CommentText) : ICommand<Guid>;