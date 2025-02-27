using Lazy.Application.Users.ResetPassword;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

public class ResetPasswordController(ISender sender, ILogger<ApiController> logger) : ApiController(sender, logger)
{

    [HttpPost("/reset-password", Name = nameof(ResetPassword))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Token, request.Email, request.NewPassword);
        Result<bool> result = await sender.Send(command);
        return result.IsFailure ? HandleFailure(result) : Ok();
    }
}