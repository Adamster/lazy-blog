﻿using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Users.CreateUser;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Lazy.Application.Users.UpdateUser;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : ApiController
{
    public UsersController(ISender sender) : base(sender)
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
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        Result<LoginResponse> tokenResult = await Sender.Send(command, cancellationToken);

        return tokenResult.IsFailure ? HandleFailure(tokenResult) : Ok(tokenResult.Value);
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
            request.LastName);

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

        var response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}