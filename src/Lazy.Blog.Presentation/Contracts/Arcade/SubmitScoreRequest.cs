using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Arcade;

public record SubmitScoreRequest(
    [Required] int Score,
    string? Game = null);
