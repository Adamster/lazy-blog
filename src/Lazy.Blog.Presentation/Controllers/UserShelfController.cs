using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/users/{username}/shelf")]
public sealed class UserShelfController(
    ISender sender,
    ILogger<UserShelfController> logger) : BaseJwtController(sender, logger)
{
    [AllowAnonymous, HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShelfCatalogResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        string username,
        [FromQuery] string? status,
        [FromQuery] string? genre,
        [FromQuery] string? search,
        [FromQuery] string? sort = null,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 24,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetUserShelfQuery(username, status, genre, search, sort, offset, limit), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}
