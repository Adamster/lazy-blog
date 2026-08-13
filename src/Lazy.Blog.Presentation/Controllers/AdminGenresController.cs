using Lazy.Domain.Entities.Identity;
using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/admin/genres")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminGenresController(ISender sender,ILogger<AdminGenresController> logger):BaseJwtController(sender,logger)
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct){var r=await Sender.Send(new GetGenresQuery(),ct);return r.IsFailure?HandleFailure(r):Ok(r.Value);}
    [HttpPost] public async Task<IActionResult> Create(ManageGenreRequest request,CancellationToken ct){var r=await Sender.Send(new CreateGenreCommand(request.Slug,request.Name,request.SortOrder),ct);return r.IsFailure?HandleFailure(r):StatusCode(StatusCodes.Status201Created,new{id=r.Value});}
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id,ManageGenreRequest request,CancellationToken ct){var r=await Sender.Send(new UpdateGenreCommand(id,request.Slug,request.Name,request.SortOrder),ct);return r.IsFailure?HandleFailure(r):NoContent();}
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct){var r=await Sender.Send(new DeleteGenreCommand(id),ct);return r.IsFailure?HandleFailure(r):NoContent();}
}
