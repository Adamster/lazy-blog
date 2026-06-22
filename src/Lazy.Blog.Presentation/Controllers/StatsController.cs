using Lazy.Application.Posts.GetHomeStats;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/stats")]
public class StatsController : BaseJwtController
{
    public StatsController(ISender sender, ILogger<StatsController> logger)
        : base(sender, logger)
    {
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetHomeStats))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HomeStatsResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHomeStats(CancellationToken ct)
    {
        var query = new GetHomeStatsQuery();

        Result<HomeStatsResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : HandleFailure(response);
    }
}
