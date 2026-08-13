using System.Text.Json;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Application.Shelf;

public record ManageBookData(Guid GenreId, Guid AuthorId, string Slug, string Title,
    string? Series, IReadOnlyList<string> SeriesTitles, string CoverUrl,
    decimal Rating, int RatingCount, string RatingSource, string? SourceWorkId, string? SourceUrl);
public record CreateBookCommand(ManageBookData Data, IFormFile Cover) : ICommand<Guid>;
public record UpdateBookCommand(Guid Id, ManageBookData Data, IFormFile? Cover) : ICommand;
public record DeleteBookCommand(Guid Id) : ICommand;
public record CreateGenreCommand(string Slug, string Name, int SortOrder) : ICommand<Guid>;
public record UpdateGenreCommand(Guid Id, string Slug, string Name, int SortOrder) : ICommand;
public record DeleteGenreCommand(Guid Id) : ICommand;
public record CreateAuthorCommand(string Name):ICommand<Guid>;
public record UpdateAuthorCommand(Guid Id,string Name):ICommand;
public record DeleteAuthorCommand(Guid Id):ICommand;

public sealed class CreateBookCommandHandler(IShelfRepository shelf, IUnitOfWork uow, IBookCoverService covers, ILogger<CreateBookCommandHandler> logger) : ICommandHandler<CreateBookCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBookCommand r, CancellationToken ct)
    {
        if (!await shelf.Genres.AnyAsync(x => x.Id == r.Data.GenreId, ct)) return Result.Failure<Guid>(DomainErrors.Shelf.GenreNotFound(r.Data.GenreId));
        if (!await shelf.Authors.AnyAsync(x => x.Id == r.Data.AuthorId, ct)) return Result.Failure<Guid>(DomainErrors.Shelf.AuthorNotFound(r.Data.AuthorId));
        var slug = Normalize(r.Data.Slug);
        if (await shelf.Books.AnyAsync(x => x.Slug == slug || (r.Data.SourceWorkId != null && x.RatingSource == r.Data.RatingSource && x.SourceWorkId == r.Data.SourceWorkId), ct))
            return Result.Failure<Guid>(DomainErrors.Shelf.BookAlreadyExists);
        var uploaded = await covers.ProcessAndUploadAsync(r.Cover, ct);
        if (uploaded is null) return Result.Failure<Guid>(DomainErrors.Shelf.CoverUploadFailed);
        var d = r.Data with { CoverUrl = uploaded.Url }; var bookResult = Book.Create(Guid.NewGuid(), d.GenreId, d.AuthorId, Normalize(d.Slug), Normalize(d.Title), d.CoverUrl);
        if(bookResult.IsFailure){await covers.DeleteAsync(uploaded.Url,CancellationToken.None);return Result.Failure<Guid>(bookResult.Error);}var book=bookResult.Value;
        var applyResult=Apply(book, d);if(applyResult.IsFailure){await covers.DeleteAsync(uploaded.Url,CancellationToken.None);return Result.Failure<Guid>(applyResult.Error);}shelf.Add(book);
        try { await uow.SaveChangesAsync(ct); return book.Id; }
        catch(DbUpdateException ex) when(IsUniqueConflict(ex)){if(!await shelf.Books.AnyAsync(x=>x.CoverUrl==uploaded.Url,CancellationToken.None))await covers.DeleteAsync(uploaded.Url,CancellationToken.None);return Result.Failure<Guid>(DomainErrors.Shelf.BookAlreadyExists);}
        catch { if(!await shelf.Books.AnyAsync(x=>x.CoverUrl==uploaded.Url,CancellationToken.None) && !await covers.DeleteAsync(uploaded.Url, CancellationToken.None))logger.LogWarning("Failed to clean up cover {CoverUrl} after book creation failed",uploaded.Url); throw; }
    }
    internal static Result Apply(Book b, ManageBookData d)
    { var result=b.UpdateSlug(Normalize(d.Slug));if(result.IsFailure)return result;result=b.UpdateCatalog(d.GenreId,d.AuthorId,Normalize(d.Title),d.CoverUrl,
        d.Series is null?null:Normalize(d.Series),JsonSerializer.Serialize(d.SeriesTitles.Select(Normalize)),d.SourceWorkId,d.SourceUrl);if(result.IsFailure)return result;return b.UpdateRating(d.Rating,d.RatingCount,d.RatingSource,DateTime.UtcNow); }
    internal static string Normalize(string value)=>string.Join(' ',value.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries));
    internal static bool IsUniqueConflict(DbUpdateException ex)=>ex.InnerException?.Message.Contains("duplicate key",StringComparison.OrdinalIgnoreCase)==true||ex.InnerException?.Message.Contains("unique index",StringComparison.OrdinalIgnoreCase)==true;
}
public sealed class UpdateBookCommandHandler(IShelfRepository shelf, IUnitOfWork uow, IBookCoverService covers, ILogger<UpdateBookCommandHandler> logger) : ICommandHandler<UpdateBookCommand>
{
    public async Task<Result> Handle(UpdateBookCommand r, CancellationToken ct)
    { var b=await shelf.Books.FirstOrDefaultAsync(x=>x.Id==r.Id,ct); if(b is null)return Result.Failure(DomainErrors.Shelf.BookNotFound(r.Id));
      if(!await shelf.Genres.AnyAsync(x=>x.Id==r.Data.GenreId,ct))return Result.Failure(DomainErrors.Shelf.GenreNotFound(r.Data.GenreId));
      if(!await shelf.Authors.AnyAsync(x=>x.Id==r.Data.AuthorId,ct))return Result.Failure(DomainErrors.Shelf.AuthorNotFound(r.Data.AuthorId));
      var slug=CreateBookCommandHandler.Normalize(r.Data.Slug);if(await shelf.Books.AnyAsync(x=>x.Id!=r.Id&&(x.Slug==slug||(r.Data.SourceWorkId!=null&&x.RatingSource==r.Data.RatingSource&&x.SourceWorkId==r.Data.SourceWorkId)),ct))return Result.Failure(DomainErrors.Shelf.BookAlreadyExists);
      var oldUrl=b.CoverUrl; BookCoverUpload? uploaded=null;
      if(r.Cover is not null){uploaded=await covers.ProcessAndUploadAsync(r.Cover,ct);if(uploaded is null)return Result.Failure(DomainErrors.Shelf.CoverUploadFailed);}
      var applyResult=CreateBookCommandHandler.Apply(b,r.Data with {CoverUrl=uploaded?.Url??oldUrl});if(applyResult.IsFailure){if(uploaded is not null)await covers.DeleteAsync(uploaded.Url,CancellationToken.None);return applyResult;}
      try{await uow.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(CreateBookCommandHandler.IsUniqueConflict(ex)){if(uploaded is not null)await covers.DeleteAsync(uploaded.Url,CancellationToken.None);return Result.Failure(DomainErrors.Shelf.BookAlreadyExists);}catch{if(uploaded is not null&&!await shelf.Books.AnyAsync(x=>x.Id!=b.Id&&x.CoverUrl==uploaded.Url,CancellationToken.None)&&!await covers.DeleteAsync(uploaded.Url,CancellationToken.None))logger.LogWarning("Failed to clean up cover {CoverUrl} after book update failed",uploaded.Url);throw;}
      if(uploaded is not null && oldUrl!=uploaded.Url && !await shelf.Books.AnyAsync(x=>x.Id!=b.Id&&x.CoverUrl==oldUrl,CancellationToken.None) && !await covers.DeleteAsync(oldUrl,CancellationToken.None))logger.LogWarning("Failed to delete replaced cover {CoverUrl}",oldUrl);
      return Result.Success(); }
}
public sealed class DeleteBookCommandHandler(IShelfRepository shelf,IUnitOfWork uow,IBookCoverService covers,ILogger<DeleteBookCommandHandler> logger):ICommandHandler<DeleteBookCommand>
{ public async Task<Result> Handle(DeleteBookCommand r,CancellationToken ct){var b=await shelf.Books.FirstOrDefaultAsync(x=>x.Id==r.Id,ct);if(b is null)return Result.Failure(DomainErrors.Shelf.BookNotFound(r.Id));var url=b.CoverUrl;shelf.Remove(b);await uow.SaveChangesAsync(ct);if(!await shelf.Books.AnyAsync(x=>x.CoverUrl==url,CancellationToken.None)&&!await covers.DeleteAsync(url,CancellationToken.None))logger.LogWarning("Failed to delete cover {CoverUrl} for deleted book {BookId}",url,r.Id);return Result.Success();}}
public sealed class CreateGenreCommandHandler(IShelfRepository shelf,IUnitOfWork uow):ICommandHandler<CreateGenreCommand,Guid>
{public async Task<Result<Guid>> Handle(CreateGenreCommand r,CancellationToken ct){var slug=CreateBookCommandHandler.Normalize(r.Slug);var name=CreateBookCommandHandler.Normalize(r.Name);if(await shelf.Genres.AnyAsync(x=>x.Slug==slug||x.Name==name,ct))return Result.Failure<Guid>(DomainErrors.Shelf.GenreAlreadyExists);var result=Genre.Create(Guid.NewGuid(),slug,name,r.SortOrder);if(result.IsFailure)return Result.Failure<Guid>(result.Error);var g=result.Value;shelf.Add(g);try{await uow.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(CreateBookCommandHandler.IsUniqueConflict(ex)){return Result.Failure<Guid>(DomainErrors.Shelf.GenreAlreadyExists);}return g.Id;}}
public sealed class UpdateGenreCommandHandler(IShelfRepository shelf,IUnitOfWork uow):ICommandHandler<UpdateGenreCommand>
{public async Task<Result> Handle(UpdateGenreCommand r,CancellationToken ct){var g=await shelf.Genres.FirstOrDefaultAsync(x=>x.Id==r.Id,ct);if(g is null)return Result.Failure(DomainErrors.Shelf.GenreNotFound(r.Id));var slug=CreateBookCommandHandler.Normalize(r.Slug);var name=CreateBookCommandHandler.Normalize(r.Name);if(await shelf.Genres.AnyAsync(x=>x.Id!=r.Id&&(x.Slug==slug||x.Name==name),ct))return Result.Failure(DomainErrors.Shelf.GenreAlreadyExists);var result=g.Update(slug,name,r.SortOrder);if(result.IsFailure)return result;try{await uow.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(CreateBookCommandHandler.IsUniqueConflict(ex)){return Result.Failure(DomainErrors.Shelf.GenreAlreadyExists);}return Result.Success();}}
public sealed class DeleteGenreCommandHandler(IShelfRepository shelf,IUnitOfWork uow):ICommandHandler<DeleteGenreCommand>
{public async Task<Result> Handle(DeleteGenreCommand r,CancellationToken ct){var g=await shelf.Genres.FirstOrDefaultAsync(x=>x.Id==r.Id,ct);if(g is null)return Result.Failure(DomainErrors.Shelf.GenreNotFound(r.Id));if(await shelf.Books.AnyAsync(x=>x.GenreId==r.Id,ct))return Result.Failure(DomainErrors.Shelf.GenreInUse);shelf.Remove(g);await uow.SaveChangesAsync(ct);return Result.Success();}}
public sealed class CreateAuthorCommandHandler(IShelfRepository shelf,IUnitOfWork uow):ICommandHandler<CreateAuthorCommand,Guid>
{public async Task<Result<Guid>> Handle(CreateAuthorCommand r,CancellationToken ct){var result=Author.Create(Guid.NewGuid(),r.Name);if(result.IsFailure)return Result.Failure<Guid>(result.Error);var a=result.Value;var existing=await shelf.Authors.FirstOrDefaultAsync(x=>x.NormalizedName==a.NormalizedName,ct);if(existing is not null)return existing.Id;shelf.Add(a);try{await uow.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(CreateBookCommandHandler.IsUniqueConflict(ex)){var raced=await shelf.Authors.AsNoTracking().FirstOrDefaultAsync(x=>x.NormalizedName==a.NormalizedName,ct);return raced is null?Result.Failure<Guid>(DomainErrors.Shelf.AuthorAlreadyExists):raced.Id;}return a.Id;}}
public sealed class UpdateAuthorCommandHandler(IShelfRepository shelf,IUnitOfWork uow):ICommandHandler<UpdateAuthorCommand>
{public async Task<Result> Handle(UpdateAuthorCommand r,CancellationToken ct){var a=await shelf.Authors.FirstOrDefaultAsync(x=>x.Id==r.Id,ct);if(a is null)return Result.Failure(DomainErrors.Shelf.AuthorNotFound(r.Id));var normalized=CreateBookCommandHandler.Normalize(r.Name).ToUpperInvariant();if(await shelf.Authors.AnyAsync(x=>x.Id!=r.Id&&x.NormalizedName==normalized,ct))return Result.Failure(DomainErrors.Shelf.AuthorAlreadyExists);var result=a.Update(r.Name);if(result.IsFailure)return result;try{await uow.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(CreateBookCommandHandler.IsUniqueConflict(ex)){return Result.Failure(DomainErrors.Shelf.AuthorAlreadyExists);}return Result.Success();}}
public sealed class DeleteAuthorCommandHandler(IShelfRepository shelf,IUnitOfWork uow):ICommandHandler<DeleteAuthorCommand>
{public async Task<Result> Handle(DeleteAuthorCommand r,CancellationToken ct){var a=await shelf.Authors.FirstOrDefaultAsync(x=>x.Id==r.Id,ct);if(a is null)return Result.Failure(DomainErrors.Shelf.AuthorNotFound(r.Id));if(await shelf.Books.AnyAsync(x=>x.AuthorId==r.Id,ct))return Result.Failure(DomainErrors.Shelf.AuthorInUse);shelf.Remove(a);await uow.SaveChangesAsync(ct);return Result.Success();}}
