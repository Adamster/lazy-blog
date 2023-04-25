using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Comments.GetCommentByPostSlug;
using Lazy.Application.Posts.AddPostView;
using Lazy.Application.Posts.AddPostVote;
using Lazy.Application.Posts.CreatePost;
using Lazy.Application.Posts.DeletePost;
using Lazy.Application.Posts.GetPostById;
using Lazy.Application.Posts.GetPostBySlug;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Posts.GetPostByUserName;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Posts.HidePost;
using Lazy.Application.Posts.PublishPost;
using Lazy.Application.Posts.UpdatePost;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Posts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Authorize]
[Route("api/posts")]
public class PostsController : ApiController
{
    public PostsController(ISender sender,
        ILogger<PostsController> logger) 
        : base(sender, logger)
    {
    }
    
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<PostResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);

        Result<PostResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<PublishedPostResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<PostDetailedResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostBySlug(string slug, CancellationToken cancellationToken)
    {
        var query = new GetPostBySlugQuery(slug);

        Result<PostDetailedResponse> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<CommentResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentForPostBySlug(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCommentByPostIdQuery(id);

        Result<List<CommentResponse>> response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{userName}/posts")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<UserPostResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Guid))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePost(
        [FromBody]CreatePostRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new CreatePostCommand(
            request.Title,
            request.Summary,
            request.Body,
            request.IsPublished,
            request.CoverUrl,
            request.UserId);

        Result<PostCreatedResponse> result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetPostBySlug),
            new {slug = result.Value.Slug},
            result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            request.Slug,
            request.CoverUrl,
            request.IsPublished);

        Result result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePostCommand(id);

        Result result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/count-view")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddView(Guid id, CancellationToken cancellationToken)
    {
        var command = new AddPostViewCommand(id);

        Result result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/vote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    
    public async Task<IActionResult> VotePost(Guid id,
        VoteDirection direction, 
        CancellationToken cancellationToken)
    {
        var command = new AddPostVoteCommand(id, direction);

        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/publish")]
    public async Task<IActionResult> PublishPost(Guid id, CancellationToken cancellationToken)
    {
        var command = new PublishPostCommand(id);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/hide")]
    public async Task<IActionResult> HidePost(Guid id, CancellationToken cancellationToken)
    {
        var command = new HidePostCommand(id);

        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    
}