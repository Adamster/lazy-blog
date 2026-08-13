using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record UpdateAuthorCommand(Guid Id, string Name) : ICommand;

public sealed class UpdateAuthorCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<UpdateAuthorCommand>
{
    public async Task<Result> Handle(UpdateAuthorCommand request, CancellationToken ct)
    {
        var value = await repository.GetAuthorAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.AuthorNotFound(request.Id));
        string normalized = ShelfInput.Normalize(request.Name).ToUpperInvariant();
        var existing = await repository.GetAuthorByNormalizedNameAsync(normalized, ct);
        if (existing is not null && existing.Id != request.Id) return Result.Failure(DomainErrors.Shelf.AuthorAlreadyExists);
        var result = value.Update(request.Name);
        if (result.IsFailure) return result;
        return await unitOfWork.SaveChangesAsync(ShelfConflict.Author, ct);
    }
}

public sealed class UpdateAuthorCommandValidator : AbstractValidator<UpdateAuthorCommand>
{
    public UpdateAuthorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}
