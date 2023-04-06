namespace Lazy.Presentation.Contracts.Comments;

public record AddCommentRequest(Guid PostId, Guid UserId, string CommentText);