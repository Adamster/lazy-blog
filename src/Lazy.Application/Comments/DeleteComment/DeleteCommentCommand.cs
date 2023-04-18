using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Comments.DeleteComment;

public record DeleteCommentCommand(Guid Id) : ICommand;