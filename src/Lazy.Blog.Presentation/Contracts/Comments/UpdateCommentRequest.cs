using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Comments;

public record UpdateCommentRequest(
    [Required] Guid UserId,
    [Required] Guid CommentId, 
    [Required] string Body);