using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Users;

public sealed record LogoutRequest(
    [Required] string RefreshToken);
