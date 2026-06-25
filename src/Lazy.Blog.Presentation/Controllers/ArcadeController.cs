using Lazy.Application.Arcade.GetLeaderboard;
using Lazy.Application.Arcade.GetMyArcadeStats;
using Lazy.Application.Arcade.SubmitScore;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Arcade;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Arcade;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/arcade")]
public class ArcadeController : BaseJwtController
{
    public ArcadeController(ISender sender, ILogger<ArcadeController> logger)
        : base(sender, logger)
    {
    }

    [HttpPost("scores", Name = nameof(SubmitScore))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ArcadeStatsResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitScore(
        [FromBody] SubmitScoreRequest request,
        CancellationToken ct)
    {
        var command = new SubmitScoreCommand(request.Game ?? GameKey.Snake.Value, request.Score);

        Result<ArcadeStatsResponse> response = await Sender.Send(command, ct);

        return response.IsSuccess ? Ok(response.Value) : HandleFailure(response);
    }

    [AllowAnonymous]
    [HttpGet("leaderboard", Name = nameof(GetLeaderboard))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeaderboardResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLeaderboard(
        CancellationToken ct,
        string? game = null,
        int take = 10)
    {
        var query = new GetLeaderboardQuery(game ?? GameKey.Snake.Value, take);

        Result<LeaderboardResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : HandleFailure(response);
    }

    [HttpGet("scores/me", Name = nameof(GetMyArcadeStats))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ArcadeStatsResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyArcadeStats(
        CancellationToken ct,
        string? game = null)
    {
        var query = new GetMyArcadeStatsQuery(game ?? GameKey.Snake.Value);

        Result<ArcadeStatsResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : HandleFailure(response);
    }
}
