using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Users.CheckIfUserNameIsUnique;
using Lazy.Application.Users.CreateUser;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Lazy.Application.Users.RefreshToken;
using Lazy.Application.Users.UpdateUser;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : ApiController
{
    public UsersController(
        ISender sender, 
        ILogger<UsersController> logger)
        : base(sender, logger)
    {
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);

        Result<UserResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{username}/available")]
    public async Task<IActionResult> CheckIfUserNameIsAvailable(string username, CancellationToken cancellationToken)
    {
        var query = new CheckIfUserNameIsUnique(username);

        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
           return HandleFailure(result);
        }

        return Ok(result.Value);
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        Result<LoginResponse> tokenResult = await Sender.Send(command, cancellationToken);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.Token, request.RefreshToken);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result);

    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.UserName,
            request.Password);

        Result<Guid> result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Username);

        Result result = await Sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            HandleFailure(result);
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/posts")]
    public async Task<IActionResult> GetPostByUserId(
        Guid id,
        [FromQuery] int offset, 
        CancellationToken cancellationToken)
    {
        var query = new GetPostByUserIdQuery(id, offset);

        Result<UserPostResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}