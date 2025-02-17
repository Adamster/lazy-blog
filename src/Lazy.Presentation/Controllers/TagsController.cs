using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Tags.SearchTag;
using Lazy.Application.Tags.UpdateTag;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;


[Route("api/tags")]
public class TagsController : ApiController
{
    public TagsController(ISender sender, ILogger<TagsController> logger) : base(sender, logger)
    {
    }

    [HttpGet("{searchTerm}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchTag(string searchTerm, CancellationToken ct)
    {
        var query = new SearchTagQuery(searchTerm);

        Result<List<TagResponse>> response = await Sender.Send(query, ct);

        return response.IsFailure ? HandleFailure(response) : Ok(response.Value);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(
        Guid id, 
        string tag,
        CancellationToken ct)
    {
        var command = new UpdateTagCommand(id, tag);

        Result result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}