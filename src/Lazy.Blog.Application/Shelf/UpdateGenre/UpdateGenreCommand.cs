using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record UpdateGenreCommand(Guid Id, string Slug, string Name, int SortOrder) : ICommand;

public sealed class UpdateGenreCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<UpdateGenreCommand>
{
    public async Task<Result> Handle(UpdateGenreCommand request, CancellationToken ct)
    {
        var value = await repository.GetGenreAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.GenreNotFound(request.Id));
        string slug = ShelfInput.Normalize(request.Slug);
        string name = ShelfInput.Normalize(request.Name);
        if ((await repository.GetGenresAsync(ct)).Any(x => x.Id != request.Id && (x.Slug == slug || x.Name == name)))
            return Result.Failure(DomainErrors.Shelf.GenreAlreadyExists);
        var result = value.Update(slug, name, request.SortOrder);
        if (result.IsFailure) return result;
        return await unitOfWork.SaveChangesAsync(ShelfConflict.Genre, ct);
    }
}

public sealed class UpdateGenreCommandValidator : AbstractValidator<UpdateGenreCommand>
{public UpdateGenreCommandValidator(){RuleFor(x=>x.Id).NotEmpty();RuleFor(x=>x.Slug).NotEmpty().MaximumLength(50);RuleFor(x=>x.Name).NotEmpty().MaximumLength(100);}}
