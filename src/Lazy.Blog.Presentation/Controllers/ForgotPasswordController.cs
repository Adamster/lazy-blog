using Lazy.Application.Users.ForgotPassword;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

public class ForgotPasswordController(ISender sender, ILogger<ApiController> logger) : ApiController(sender, logger)
{
    [HttpPost("/forgot-password", Name = nameof(ForgotPassword))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await Sender.Send(command);
        return Accepted();
    }
}