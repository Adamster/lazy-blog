using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Tags.GetAllTags;
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

    [HttpGet("{searchTerm}", Name = "SearchTags")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchTag(string searchTerm, CancellationToken ct)
    {
        var query = new SearchTagQuery(searchTerm);

        Result<List<TagResponse>> response = await Sender.Send(query, ct);

        return response.IsFailure ? HandleFailure(response) : Ok(response.Value);
    }

    [Authorize]
    [HttpPut("{id:guid}", Name = "UpdateTag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTag(
        Guid id, 
        string tag,
        CancellationToken ct)
    {
        var command = new UpdateTagCommand(id, tag);

        Result result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetTags))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTags(CancellationToken ct)
    {
        var query = new GetAllTagsQuery();
        
        var result = await  Sender.Send(query, ct);
        
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }
}