using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Users.CheckIfUserNameIsUnique;
using Lazy.Application.Users.CreateUser;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Lazy.Application.Users.RefreshToken;
using Lazy.Application.Users.UpdateUser;
using Lazy.Application.Users.UploadUserAvatar;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var query = new GetUserByIdQuery(id);

        Result<UserResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{username}/available", Name = "CheckUserAvailability")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<IActionResult> CheckIfUserNameIsAvailable(string username, CancellationToken ct)
    {
        var query = new CheckIfUserNameIsUnique(username);

        var result = await Sender.Send(query, ct);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/avatar", Name = "UploadUserAvatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAvatar(
        [FromRoute] Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        var command = new UploadUserAvatarCommand(id, file);

        var result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login", Name = "Login")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);

        Result<LoginResponse> tokenResult = await Sender.Send(command, ct);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
    }

    [AllowAnonymous]
    [HttpPost("refresh", Name = "RefreshToken")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("register", Name = "Register")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterUser(
        [FromBody] RegisterUserRequest request,
        CancellationToken ct)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.FirstName,
            request.LastName,
            request.UserName,
            request.Password);

        Result<Guid> result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Value },
            result.Value);
    }

    [HttpPut("{id:guid}", Name = "UpdateUser")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.UserName);

        Result result = await Sender.Send(
            command,
            ct);

        if (result.IsFailure)
        {
            HandleFailure(result);
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/posts", Name = "GetPostsByUserId")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPostResponse))]
    public async Task<IActionResult> GetPostByUserId(
        Guid id,
        [FromQuery] int offset,
        CancellationToken ct)
    {
        var query = new GetPostByUserIdQuery(id, offset);

        Result<UserPostResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}