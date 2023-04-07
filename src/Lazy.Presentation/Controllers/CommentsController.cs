using Lazy.Application.Comments;
using Lazy.Application.Comments.AddComment;
using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Comments.UpdateComment;
using Lazy.Domain.Shared;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Comments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazy.Presentation.Controllers;


[Authorize]
[Route("api/comments")]
public class CommentsController : ApiController
{
    public CommentsController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetCommentById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCommentByIdQuery(id);

        var response = await Sender.Send(query, cancellationToken);

        return response.IsSuccess ? Ok(response.Value) : NotFound(response.Error);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody]AddCommentRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new AddCommentCommand(
            request.PostId,
            request.UserId,
            request.CommentText);

        var result = await Sender.Send(command, cancellationToken);

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
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCommentCommand(
            request.UserId,
            request.CommentId,
            request.CommentText);

        Result result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result);
        }

        return NoContent();
    }
}