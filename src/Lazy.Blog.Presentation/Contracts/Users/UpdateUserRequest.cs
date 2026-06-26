using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Users;

public record UpdateUserRequest(
   [Required] string DisplayName,
   [Required] string UserName,
    string? Biography);
