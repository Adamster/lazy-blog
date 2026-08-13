using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record CreateGenreCommand(string Slug, string Name, int SortOrder) : ICommand<Guid>;

public sealed class CreateGenreCommandHandler(
    IGenreRepository genres,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<CreateGenreCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateGenreCommand request, CancellationToken ct)
    {
        string slug = ShelfInput.Normalize(request.Slug);
        string name = ShelfInput.Normalize(request.Name);
        if (await genres.ConflictsAsync(null, slug, name, ct))
            return Result.Failure<Guid>(DomainErrors.Shelf.GenreAlreadyExists);
        var result = Genre.Create(Guid.NewGuid(), slug, name, request.SortOrder);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        genres.Add(result.Value);
        var saveResult = await unitOfWork.SaveChangesAsync(ShelfConflict.Genre, ct);
        return saveResult.IsFailure ? Result.Failure<Guid>(saveResult.Error) : result.Value.Id;
    }
}

public sealed class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
