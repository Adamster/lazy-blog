using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record CreateAuthorCommand(string Name) : ICommand<Guid>;

public sealed class CreateAuthorCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<CreateAuthorCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAuthorCommand request, CancellationToken ct)
    {
        var result = Author.Create(Guid.NewGuid(), request.Name);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        var existing = await repository.GetAuthorByNormalizedNameAsync(result.Value.NormalizedName, ct);
        if (existing is not null) return existing.Id;
        repository.Add(result.Value);
        var saveResult = await unitOfWork.SaveChangesAsync(ShelfConflict.Author, ct);
        return saveResult.IsFailure ? Result.Failure<Guid>(saveResult.Error) : result.Value.Id;
    }
}

public sealed class CreateAuthorCommandValidator:AbstractValidator<CreateAuthorCommand>{public CreateAuthorCommandValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(300);}}
