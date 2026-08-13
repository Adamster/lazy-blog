using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/book-comments")]
public sealed class BookCommentsController(ISender sender, ILogger<BookCommentsController> logger) : BaseJwtController(sender, logger)
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, BookCommentRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateBookCommentCommand(id, request.Body), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteBookCommentCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }
}
