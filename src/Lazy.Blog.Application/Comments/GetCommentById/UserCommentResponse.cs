namespace Lazy.Application.Comments.GetCommentById;

public record UserCommentResponse(
    Guid Id,
    string DisplayName,
    string UserName,
    string? AvatarUrl);
