using Lazy.Application.Posts.CreatePost;
using Lazy.Application.Posts.GetPostById;
using Lazy.Application.Posts.GetPostBySlug;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Posts.UpdatePost;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Posts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazy.Presentation.Controllers;

[Authorize]
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

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPosts(int offset, CancellationToken cancellationToken)
    {
        var query = new GetPublishedPostsQuery(offset);
        Result<List<PublishedPostResponse>> result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPostBySlug(GetPostBySlugRequest request, CancellationToken cancellationToken)
    {
        var query = new GetPostBySlugQuery(request.Slug);

        Result<PostDetailedResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(
        [FromBody]CreatePostRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new CreatePostCommand(
            request.Title,
            request.Summary,
            request.Body,
            request.UserId);

        Result<Guid> result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetPostById),
            new {id = result.Value},
            result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePost(
        Guid id,
        [FromBody] UpdatePostRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdatePostCommand(
            id,
            request.Title,
            request.Summary,
            request.Body);

        Result result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}