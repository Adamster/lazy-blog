using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/users/{username}/books")]
public sealed class UserBooksController(ISender sender, ILogger<UserBooksController> logger) : BaseJwtController(sender, logger)
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Get(
        string username,
        [FromQuery] string status,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 24,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetUserBooksQuery(username, status, offset, limit), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}
