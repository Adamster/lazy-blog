using Lazy.Application.Comments.GetCommentByPostSlug;
using Lazy.Application.Posts.CreatePost;
using Lazy.Application.Posts.GetPostById;
using Lazy.Application.Posts.GetPostBySlug;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Posts.GetPostByUserName;
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
    
    [AllowAnonymous]
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
    public async Task<IActionResult> GetPostBySlug(string slug, CancellationToken cancellationToken)
    {
        var query = new GetPostBySlugQuery(slug);

        Result<PostDetailedResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/comments")]
    public async Task<IActionResult> GetCommentForPostBySlug(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCommentByPostIdQuery(id);

        var response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{userName}/posts")]
    public async Task<IActionResult> GetPostByUserName(
        string userName,
        [FromQuery]int offset,
        CancellationToken cancellationToken)
    {
        var query = new GetPostByUserNameQuery(userName, offset);

        Result<UserPostResponse> response = await Sender.Send(query, cancellationToken);

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
            request.IsPublished,
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
            request.Body,
            request.Slug);

        Result result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}