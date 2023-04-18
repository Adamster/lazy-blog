namespace Lazy.Presentation.Contracts.Comments;

public record UpdateCommentRequest(Guid UserId, Guid CommentId, string Body);