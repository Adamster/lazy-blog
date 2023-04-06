namespace Lazy.Application.Comments.GetCommentById;

public record CommentResponse(
    Guid CommentId,
    string Username,
    string UserAvatarUrl,
    string CommentText,
    DateTime CreatedAtUtc
);