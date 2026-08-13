using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/series")]
public sealed class SeriesController(
    ISender sender,
    ILogger<SeriesController> logger) : BaseJwtController(sender, logger)
{
    [AllowAnonymous, HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<SeriesResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await Sender.Send(new GetSeriesQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [AllowAnonymous, HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SeriesDetailResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await Sender.Send(new GetSeriesBySlugQuery(slug), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}
