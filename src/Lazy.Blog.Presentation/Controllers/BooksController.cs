using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/books")]
public sealed class BooksController(ISender sender, ILogger<BooksController> logger) : BaseJwtController(sender, logger)
{
    [AllowAnonymous, HttpGet("authors")]
    public async Task<IActionResult> GetAuthors([FromQuery] string? query, CancellationToken ct)
    {
        var result = await Sender.Send(new GetBookAuthorsQuery(query), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? genre,
        [FromQuery] string? search,
        [FromQuery] string? sort = null,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 24,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetBooksQuery(genre, search, sort, offset, limit), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [AllowAnonymous, HttpGet("{slug}")]
    public async Task<IActionResult> GetBook(string slug, CancellationToken ct)
    {
        var result = await Sender.Send(new GetBookBySlugQuery(slug), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, SetBookStatusRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new SetBookStatusCommand(id, request.Status), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}/status")]
    public async Task<IActionResult> RemoveStatus(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new RemoveBookStatusCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [AllowAnonymous, HttpGet("{id:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new GetBookCommentsQuery(id), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, BookCommentRequest request, CancellationToken ct)
    {
        var result = await Sender.Send(new AddBookCommentCommand(id, request.Body), ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Created($"/api/book-comments/{result.Value}", new { id = result.Value });
    }
}
