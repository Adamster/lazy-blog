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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<BookAuthorResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] string? query, CancellationToken ct)
    {
        var result = await Sender.Send(new GetBookAuthorsQuery(query), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdResponse))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IdResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(ManageAuthorRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new CreateAuthorCommand(request.Slug, request.Name, request.NameEn), ct);
        if (result.IsFailure) return HandleFailure(result);
        var response = new IdResponse(result.Value.Id);
        return result.Value.Created
            ? Created($"/api/admin/authors/{result.Value.Id}", response)
            : Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, ManageAuthorRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateAuthorCommand(id, request.Slug, request.Name, request.NameEn), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteAuthorCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
