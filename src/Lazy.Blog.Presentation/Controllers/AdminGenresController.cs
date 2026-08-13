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

[Route("api/admin/genres")]
[Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminGenresController(ISender sender, ILogger<AdminGenresController> logger)
    : BaseJwtController(sender, logger)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<GenreResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await Sender.Send(new GetGenresQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(ManageGenreRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateGenreCommand(request.Slug, request.Name, request.SortOrder), ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Created($"/api/admin/genres/{result.Value}", new IdResponse(result.Value));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, ManageGenreRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateGenreCommand(id, request.Slug, request.Name, request.SortOrder), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteGenreCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
