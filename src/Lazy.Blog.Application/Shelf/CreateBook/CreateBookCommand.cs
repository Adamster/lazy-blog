using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record CreateBookCommand(ManageBookData Data, FileUpload Cover) : ICommand<Guid>;

public sealed class CreateBookCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork, IBookCoverService covers) : ICommandHandler<CreateBookCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBookCommand request, CancellationToken ct)
    {
        var error = await ShelfInput.Relations(repository, request.Data, ct);
        if (error is not null) return Result.Failure<Guid>(error);
        string slug = ShelfInput.Normalize(request.Data.Slug);
        if (await repository.BookConflictsAsync(null, slug, request.Data.RatingSource, request.Data.SourceWorkId, ct)) return Result.Failure<Guid>(DomainErrors.Shelf.BookAlreadyExists);
        var cover = await covers.ProcessAndUploadAsync(request.Cover, ct);
        if (cover is null) return Result.Failure<Guid>(DomainErrors.Shelf.CoverUploadFailed);
        var data = request.Data with { Slug = slug, CoverUrl = cover.Url };
        var created = Book.Create(Guid.NewGuid(), data.GenreId, data.AuthorId, slug, ShelfInput.Normalize(data.Title), cover.Url);
        if (created.IsFailure) return Result.Failure<Guid>(created.Error);
        var updated = ShelfInput.Apply(created.Value, data);
        if (updated.IsFailure) return Result.Failure<Guid>(updated.Error);
        repository.Add(created.Value);
        var saveResult = await unitOfWork.SaveChangesAsync(ShelfConflict.Book, ct);
        return saveResult.IsFailure ? Result.Failure<Guid>(saveResult.Error) : created.Value.Id;
    }
}

public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Data).SetValidator(new ManageBookDataValidator(false));
        RuleFor(x => x.Cover).NotNull().Must(x => x.Length is > 0 and <= 20 * 1024 * 1024);
    }
}
