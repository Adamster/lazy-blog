using Lazy.Application.Shelf;
using Lazy.Domain.Entities.Identity;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/admin/series")]
[Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminSeriesController(ISender sender, ILogger<AdminSeriesController> logger)
    : BaseJwtController(sender, logger)
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await Sender.Send(new GetSeriesQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ManageSeriesRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateSeriesCommand(request.Slug, request.Title), ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Created($"/api/admin/series/{result.Value}", new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ManageSeriesRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateSeriesCommand(id, request.Slug, request.Title), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteSeriesCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
