using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Users;

public sealed record RegisterUserRequest(
    [Required] string Email,
    [Required] string DisplayName,
    [Required] string UserName,
    string? Biography,
    [Required] string Password);
