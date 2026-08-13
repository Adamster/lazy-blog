using FluentValidation;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record UpdateBookCommand(Guid Id, ManageBookData Data, FileUpload? Cover) : ICommand;

public sealed class UpdateBookCommandHandler(
    IBookRepository books,
    IGenreRepository genres,
    IAuthorRepository authors,
    ISeriesRepository series,
    IShelfUnitOfWork unitOfWork,
    IBookCoverService covers) : ICommandHandler<UpdateBookCommand>
{
    public async Task<Result> Handle(UpdateBookCommand request, CancellationToken ct)
    {
        var book = await books.GetAsync(request.Id, ct);
        if (book is null) return Result.Failure(DomainErrors.Shelf.BookNotFound(request.Id));
        var error = await ShelfInput.Relations(genres, authors, series, request.Data, ct);
        if (error is not null) return Result.Failure(error);
        string slug = ShelfInput.Normalize(request.Data.Slug);
        if (await books.ConflictsAsync(request.Id, slug, request.Data.RatingSource, request.Data.SourceWorkId, ct))
            return Result.Failure(DomainErrors.Shelf.BookAlreadyExists);
        string coverUrl = book.CoverUrl;
        if (request.Cover is not null)
        {
            var cover = await covers.ProcessAndUploadAsync(request.Cover, ct);
            if (cover is null)
            {
                return Result.Failure(DomainErrors.Shelf.CoverUploadFailed);
            }

            coverUrl = cover.Url;
        }
        var result = ShelfInput.Apply(book, request.Data with { Slug = slug, CoverUrl = coverUrl });
        if (result.IsFailure) return result;
        return await unitOfWork.SaveChangesAsync(ShelfConflict.Book, ct);
    }
}

public sealed class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data).SetValidator(new ManageBookDataValidator(false));
        When(x => x.Cover is not null, () =>
            RuleFor(x => x.Cover!).Must(x => x.Length is > 0 and <= 20 * 1024 * 1024));
    }
}
