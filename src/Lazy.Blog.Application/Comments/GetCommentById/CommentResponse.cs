using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Comments.GetCommentById;

public record CommentResponse(
    Guid Id,
    UserCommentResponse User,
    string Body,
    DateTime CreatedAtUtc);
