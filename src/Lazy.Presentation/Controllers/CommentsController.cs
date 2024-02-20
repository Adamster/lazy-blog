using Lazy.Application.Comments;
using Lazy.Application.Comments.AddComment;
using Lazy.Application.Comments.DeleteComment;
using Lazy.Application.Comments.GetCommentById;
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
public class CommentsController : ApiController
{
    public CommentsController(
        ISender sender,
        ILogger<CommentsController> logger) 
        : base(sender, logger)
    {
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCommentById(Guid id, CancellationToken ct)
    {
        var query = new GetCommentByIdQuery(id);

        var response = await Sender.Send(query, ct);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [HttpPost]
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

    [HttpPut]
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request, CancellationToken ct)
    {
        var command = new UpdateCommentCommand(
            request.UserId,
            request.CommentId,
            request.Body);

        Result result = await Sender.Send(command, ct);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken ct)
    {
        var command = new DeleteCommentCommand(id);

        Result result = await Sender.Send(command, ct);

        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}