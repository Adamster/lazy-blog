using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Comments;

public record AddCommentRequest(
    [Required] Guid PostId,
    [Required] Guid UserId,
    [Required] string Body);