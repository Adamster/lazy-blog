using Lazy.Application.Shelf;
using Lazy.Domain.Entities.Identity;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/admin/genres")]
[Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminGenresController(ISender sender, ILogger<AdminGenresController> logger)
    : BaseJwtController(sender, logger)
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await Sender.Send(new GetGenresQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ManageGenreRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateGenreCommand(request.Slug, request.Name, request.SortOrder), ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Created($"/api/admin/genres/{result.Value}", new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ManageGenreRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateGenreCommand(id, request.Slug, request.Name, request.SortOrder), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteGenreCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
