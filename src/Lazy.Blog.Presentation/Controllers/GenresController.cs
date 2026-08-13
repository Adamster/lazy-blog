using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/genres")]
public sealed class GenresController(ISender sender, ILogger<GenresController> logger) : BaseJwtController(sender, logger)
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    { var result = await Sender.Send(new GetGenresQuery(), ct); return result.IsFailure ? HandleFailure(result) : Ok(result.Value); }
}
