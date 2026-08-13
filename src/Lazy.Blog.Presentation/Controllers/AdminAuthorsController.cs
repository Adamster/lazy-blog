using Lazy.Application.Shelf;
using Lazy.Domain.Entities.Identity;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/admin/authors")]
[Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminAuthorsController(ISender sender, ILogger<AdminAuthorsController> logger)
    : BaseJwtController(sender, logger)
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? query, CancellationToken ct)
    {
        var result = await Sender.Send(new GetBookAuthorsQuery(query), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ManageAuthorRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateAuthorCommand(request.Name), ct);
        return result.IsFailure ? HandleFailure(result) : StatusCode(StatusCodes.Status201Created, new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ManageAuthorRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateAuthorCommand(id, request.Name), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteAuthorCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
