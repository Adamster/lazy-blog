using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Comments.GetCommentById;

public record CommentResponse(
    Guid Id,
    UserResponse User,
    string UserAvatarUrl,
    string Body,
    DateTime CreatedAtUtc
);