﻿using Lazy.Application.Media.CreateMedia;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/media")]
public class MediaController : ApiController
{
    public MediaController(ISender sender, 
        ILogger<MediaController> logger) : base(sender, logger)
    {
    }

    [HttpPost("{id:guid}/upload", Name = "UploadMedia")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadMediaBlob([FromRoute]Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        var command = new CreateMediaCommand(id, file);
        var result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}