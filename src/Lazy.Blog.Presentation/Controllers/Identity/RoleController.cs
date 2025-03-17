using Lazy.Application.Identity.Roles.CreateRole;
using Lazy.Domain.Entities.Identity;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers.Identity;



[Authorize(Roles = "Admin")]
public class RoleController(ISender sender, ILogger<ApiController> logger) : ApiController(sender, logger)
{
   
    [HttpPost("CreateRole", Name = "CreateRole")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        var command = new CreateRoleCommand(roleName);
        
        var result = await Sender.Send(command);

        return result.IsSuccess ? Ok() : HandleFailure(result);
    }
}