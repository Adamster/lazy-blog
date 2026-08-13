using Lazy.Application.Abstractions.File;
using Lazy.Application.Shelf;
using Lazy.Domain.Entities.Identity;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lazy.Presentation.Controllers;

[Route("api/admin/books")]
[Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminBooksController(ISender sender, ILogger<AdminBooksController> logger)
    : BaseJwtController(sender, logger)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BooksPageResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? genre,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetBooksQuery(genre, search, null, offset, limit), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IdResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] ManageBookFormRequest request, CancellationToken ct)
    {
        if (request.Cover is null) return BadRequest("cover is required");
        var result = await Sender.Send(new CreateBookCommand(Map(request), Map(request.Cover)), ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Created($"/api/admin/books/{result.Value}", new IdResponse(result.Value));
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromForm] ManageBookFormRequest request, CancellationToken ct)
    {
        var cover = request.Cover is null ? null : Map(request.Cover);
        var result = await Sender.Send(new UpdateBookCommand(id, Map(request), cover), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new DeleteBookCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : NoContent();
    }

    [HttpPost("ratings/sync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RatingSyncResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncRatings(ImportBookRatingsRequest request, CancellationToken ct)
    {
        var items = request.Items
            .Select(item => new RatingImportItem(item.SourceWorkId, item.Rating, item.RatingCount))
            .ToList();
        var result = await Sender.Send(new ImportBookRatingsCommand(request.Source, items), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

    private static ManageBookData Map(ManageBookFormRequest request) =>
        new(request.GenreId, request.AuthorId, request.Slug, request.Title, request.SeriesId, request.VolumeNumber,
            string.Empty, request.Rating, request.RatingCount,
            request.RatingSource, request.SourceWorkId, request.SourceUrl);

    private static FileUpload Map(IFormFile file) =>
        new(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
}
