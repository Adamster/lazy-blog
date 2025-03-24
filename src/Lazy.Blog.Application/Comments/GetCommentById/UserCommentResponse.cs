namespace Lazy.Application.Comments.GetCommentById;

public record UserCommentResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string UserName,
    string? AvatarUrl);