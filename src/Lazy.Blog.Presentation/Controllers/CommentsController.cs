using Lazy.Application.Comments;
using Lazy.Application.Comments.AddComment;
using Lazy.Application.Comments.DeleteComment;
using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Comments.GetCommentByPostSlug;
using Lazy.Application.Comments.UpdateComment;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Comments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;


[Authorize]
[Route("api/comments")]
public class CommentsController : BaseJwtController
{
    public CommentsController(
        ISender sender,
        ILogger<CommentsController> logger) 
        : base(sender, logger)
    {
    }

    [HttpGet("{id:guid}", Name = nameof(GetCommentById))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CommentResponse))]
    public async Task<IActionResult> GetCommentById(Guid id, CancellationToken ct)
    {
        var query = new GetCommentByIdQuery(id);

        var response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [HttpPost(Name = nameof(AddComment))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddComment([FromBody]AddCommentRequest request, 
        CancellationToken ct)
    {
        var command = new AddCommentCommand(
            request.PostId,
            request.UserId,
            request.Body);

        var result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
           return HandleFailure(result);
        }

        return CreatedAtAction(
            nameof(GetCommentById),
            new { id = result.Value },
            result.Value);
    }

    [HttpPut(Name = nameof(UpdateComment))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request, CancellationToken ct)
    {
        var command = new UpdateCommentCommand(
            request.UserId,
            request.CommentId,
            request.Body);

        Result result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}", Name = nameof(DeleteComment))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken ct)
    {
        var command = new DeleteCommentCommand(id);

        Result result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();

    }
    
    [AllowAnonymous]
    [Route("~/api/posts/{id:guid}/comments", Name = nameof(GetCommentsByPostId))]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CommentResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentsByPostId(Guid id, CancellationToken ct)
    {
        var query = new GetCommentByPostIdQuery(id);

        Result<List<CommentResponse>> response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }
}