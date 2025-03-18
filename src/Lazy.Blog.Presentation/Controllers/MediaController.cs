using Lazy.Application.Media.CreateMedia;
using Lazy.Application.Media.GetMediaByUserId;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Media;
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

    [HttpGet("{userId:guid}/list", Name = "ListMedia")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MediaItemResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListMedia([FromRoute] Guid userId, CancellationToken ct)
    {
        var query = new GetMediaItemsByUserIdQuery(userId);
        
        var result = await Sender.Send(query, ct);
        
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}