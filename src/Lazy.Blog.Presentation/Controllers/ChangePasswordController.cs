using Lazy.Application.Users.ChangePassword;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
public class ChangePasswordController(ISender sender, ILogger<ApiController> logger) : ApiController(sender, logger)
{
    
    [HttpPost("/change-password", Name = nameof(ChangePassword))]
    
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(request.OldPassword, request.NewPassword);
        
        var result  = await Sender.Send(command);

        if (!result.IsSuccess)
        {
            return HandleFailure(result);
        }

        return Ok();
    }
}