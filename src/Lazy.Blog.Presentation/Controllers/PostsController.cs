using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Comments.GetCommentByPostSlug;
using Lazy.Application.Posts.AddPostView;
using Lazy.Application.Posts.AddPostVote;
using Lazy.Application.Posts.CreatePost;
using Lazy.Application.Posts.DeletePost;
using Lazy.Application.Posts.GetPostById;
using Lazy.Application.Posts.GetPostBySlug;
using Lazy.Application.Posts.GetPostByTag;
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
using Microsoft.AspNetCore.Http.HttpResults;
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
    [HttpGet("{id:guid}", Name = nameof(GetPostById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PostResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(Guid id, CancellationToken ct)
    {
        var query = new GetPostByIdQuery(id);

        Result<PostResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetAllPosts))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DisplayPostResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> GetAllPosts(int offset, CancellationToken ct)
    {
        var query = new GetPublishedPostsQuery(offset);
        Result<List<DisplayPostResponse>> result = await Sender.Send(query, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [AllowAnonymous]
    [HttpGet("t/{tag}", Name = nameof(GetPostsByTag))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DisplayPostResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPostsByTag(string tag, int offset, CancellationToken ct)
    {
        var query = new GetPostByTagQuery(tag, offset);
        Result<List<DisplayPostResponse>> result = await Sender.Send(query, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [AllowAnonymous]
    [HttpGet("{slug}", Name = nameof(GetPostBySlug))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PostDetailedResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostBySlug(string slug, CancellationToken ct)
    {
        var query = new GetPostBySlugQuery(slug);

        Result<PostDetailedResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [AllowAnonymous]
    [HttpGet("{userName}/posts", Name = nameof(GetPostsByUserName))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserPostResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostsByUserName(
        string userName,
        [FromQuery]int offset,
        CancellationToken ct)
    {
        var query = new GetPostByUserNameQuery(userName, offset);

        Result<UserPostResponse> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);

    }

    [HttpPost(Name = nameof(CreatePost))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePost(
        [FromBody]CreatePostRequest request, 
        CancellationToken ct)
    {
        var command = new CreatePostCommand(
            request.Title,
            request.Summary,
            request.Body,
            request.IsPublished,
            request.Tags,
            request.CoverUrl,
            request.UserId);

        Result<PostCreatedResponse> result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetPostBySlug),
            new {slug = result.Value.Slug},
            result.Value);
    }

    [HttpPut("{id:guid}", Name = nameof(UpdatePost))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePost(
        Guid id,
        [FromBody] UpdatePostRequest request, 
        CancellationToken ct)
    {
        var command = new UpdatePostCommand(
            id,
            request.Title,
            request.Summary,
            request.Body,
            request.Slug,
            request.CoverUrl,
            request.Tags,
            request.IsPublished);

        Result result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = nameof(DeletePost))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken ct)
    {
        var command = new DeletePostCommand(id);

        Result result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/count-view", Name = "IncrementViewCount")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddView(Guid id, CancellationToken ct)
    {
        var command = new AddPostViewCommand(id);

        Result result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/vote", Name = "VotePost")]
    [ProducesResponseType(type:typeof(NoContent),StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    
    public async Task<IActionResult> VotePost(Guid id,
        VoteDirection direction, 
        CancellationToken ct)
    {
        var command = new AddPostVoteCommand(id, direction);

        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/publish", Name = "PublishPost")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PublishPost(Guid id, CancellationToken ct)
    {
        var command = new PublishPostCommand(id);
        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/hide", Name = "HidePost")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> HidePost(Guid id, CancellationToken ct)
    {
        var command = new HidePostCommand(id);

        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}