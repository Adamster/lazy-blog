using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/series")]
public sealed class SeriesController(ISender sender, ILogger<SeriesController> logger) : BaseJwtController(sender, logger)
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await Sender.Send(new GetSeriesQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [AllowAnonymous, HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await Sender.Send(new GetSeriesBySlugQuery(slug), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}
