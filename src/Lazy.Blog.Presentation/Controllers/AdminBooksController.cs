using Lazy.Application.Shelf;
using Lazy.Presentation.Abstractions;
using Lazy.Presentation.Contracts.Shelf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Lazy.Domain.Entities.Identity;

namespace Lazy.Presentation.Controllers;

[Route("api/admin/books")]
[Authorize(Roles = Role.AdminRoleName)]
public sealed class AdminBooksController(ISender sender, ILogger<AdminBooksController> logger) : BaseJwtController(sender, logger)
{
    private static ManageBookData Map(ManageBookFormRequest x) => new(x.GenreId,x.AuthorId,x.Slug,x.Title,x.Series,x.SeriesTitles,string.Empty,x.Rating,x.RatingCount,x.RatingSource,x.SourceWorkId,x.SourceUrl);
    [HttpGet]
    public async Task<IActionResult> List([FromQuery]string? search,[FromQuery]string? genre,[FromQuery]int offset=0,[FromQuery]int limit=50,CancellationToken ct=default)
    {var result=await Sender.Send(new GetBooksQuery(genre,search,null,offset,limit),ct);return result.IsFailure?HandleFailure(result):Ok(result.Value);}
    [HttpPost,Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm]ManageBookFormRequest request,CancellationToken ct)
    {if(request.Cover is null)return BadRequest("cover is required");var result=await Sender.Send(new CreateBookCommand(Map(request),request.Cover),ct);return result.IsFailure?HandleFailure(result):StatusCode(StatusCodes.Status201Created,new{id=result.Value});}
    [HttpPut("{id:guid}"),Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id,[FromForm]ManageBookFormRequest request,CancellationToken ct)
    {var result=await Sender.Send(new UpdateBookCommand(id,Map(request),request.Cover),ct);return result.IsFailure?HandleFailure(result):NoContent();}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id,CancellationToken ct)
    {var result=await Sender.Send(new DeleteBookCommand(id),ct);return result.IsFailure?HandleFailure(result):NoContent();}
    [HttpPost("ratings/sync")]
    public async Task<IActionResult> SyncRatings(ImportBookRatingsRequest request, CancellationToken ct)
    {
        var items = request.Items.Select(x => new RatingImportItem(x.SourceWorkId, x.Rating, x.RatingCount)).ToList();
        var result = await Sender.Send(new ImportBookRatingsCommand(request.Source, items), ct);
        return result.IsFailure ? HandleFailure(result) : Ok(result.Value);
    }

}
