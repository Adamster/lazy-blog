using Lazy.Application.Shelf;using Lazy.Domain.Entities.Identity;using Lazy.Presentation.Abstractions;using Lazy.Presentation.Contracts.Shelf;using MediatR;using Microsoft.AspNetCore.Http;using Microsoft.AspNetCore.Mvc;using Microsoft.Extensions.Logging;
namespace Lazy.Presentation.Controllers;
[Route("api/admin/authors")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminAuthorsController(ISender sender,ILogger<AdminAuthorsController> logger):BaseJwtController(sender,logger)
{
[HttpGet]public async Task<IActionResult> List([FromQuery]string? query,CancellationToken ct){var r=await Sender.Send(new GetBookAuthorsQuery(query),ct);return r.IsFailure?HandleFailure(r):Ok(r.Value);}
[HttpPost]public async Task<IActionResult>Create(ManageAuthorRequest q,CancellationToken ct){var r=await Sender.Send(new CreateAuthorCommand(q.Name),ct);return r.IsFailure?HandleFailure(r):StatusCode(StatusCodes.Status201Created,new{id=r.Value});}
[HttpPut("{id:guid}")]public async Task<IActionResult>Update(Guid id,ManageAuthorRequest q,CancellationToken ct){var r=await Sender.Send(new UpdateAuthorCommand(id,q.Name),ct);return r.IsFailure?HandleFailure(r):NoContent();}
[HttpDelete("{id:guid}")]public async Task<IActionResult>Delete(Guid id,CancellationToken ct){var r=await Sender.Send(new DeleteAuthorCommand(id),ct);return r.IsFailure?HandleFailure(r):NoContent();}}
