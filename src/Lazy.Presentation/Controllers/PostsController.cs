using Lazy.Application.Posts.GetPostById;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Lazy.Presentation.Controllers;

[Route("api/posts")]
public class PostsController : ApiController
{
    public PostsController(ISender sender) : base(sender)
    {
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPostById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);

        Result<PostResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}