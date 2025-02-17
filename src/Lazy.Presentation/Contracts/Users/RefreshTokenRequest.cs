using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Users;

public sealed record RefreshTokenRequest(
    [Required] string RefreshToken);