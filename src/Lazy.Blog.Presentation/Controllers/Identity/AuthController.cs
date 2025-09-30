using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Lazy.Application.Abstractions.User;
using Lazy.Application.Identity.ExternalAuth;
using Lazy.Application.Users.Login;
using Lazy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Presentation.Controllers.Identity;


[ApiController]
[Route("auth")]
public class AuthController(ISender sender, SignInManager<User> signInManager, IUserService userService, ILogger<ApiController> logger) : ApiController(sender, logger)
{
    [HttpGet("{provider}/login")]
    public IActionResult ExternalLogin([FromRoute] string provider, string? returnUrl)
    {
        var props = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);

        return Challenge(props, provider);
    }

    [AllowAnonymous]
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallback()
    {
        logger.LogInformation("This is a callback ");


        var externalAuth = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!externalAuth.Succeeded)
        {
            return BadRequest("External cooking missing/invalid");
        }

        ExternalLoginInfo? info = await signInManager.GetExternalLoginInfoAsync();

        LoginResponse? loginResponse;
        if (info != null)
        {
            loginResponse = await userService.CreateOrLoginUser(info);
        }
        else
        {
            return BadRequest("Something went wrong with external login");
        }
        return Ok(loginResponse);
    }
}