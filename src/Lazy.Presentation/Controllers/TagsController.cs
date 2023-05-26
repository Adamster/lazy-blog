using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Tags.SearchTag;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;


[Microsoft.AspNetCore.Components.Route("api/tags")]
public class TagsController : ApiController
{
    public TagsController(ISender sender, ILogger<TagsController> logger) : base(sender, logger)
    {
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<TagResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchTag(string searchTerm, CancellationToken cancellationToken)
    {
        var query = new SearchTagQuery(searchTerm);

        Result<List<TagResponse>> response = await Sender.Send(query, cancellationToken);

        return response.IsFailure ? HandleFailure(response) : Ok(response.Value);
    }
}