using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record CreateAuthorCommand(string Name) : ICommand<CreateAuthorResult>;

public record CreateAuthorResult(Guid Id, bool Created);

public sealed class CreateAuthorCommandHandler(
    IAuthorRepository authors,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<CreateAuthorCommand, CreateAuthorResult>
{
    public async Task<Result<CreateAuthorResult>> Handle(CreateAuthorCommand request, CancellationToken ct)
    {
        var result = Author.Create(Guid.NewGuid(), request.Name);
        if (result.IsFailure) return Result.Failure<CreateAuthorResult>(result.Error);
        var existing = await authors.GetByNormalizedNameAsync(result.Value.NormalizedName, ct);
        if (existing is not null) return new CreateAuthorResult(existing.Id, false);
        authors.Add(result.Value);
        var saveResult = await unitOfWork.SaveChangesAsync(ShelfConflict.Author, ct);
        return saveResult.IsFailure
            ? Result.Failure<CreateAuthorResult>(saveResult.Error)
            : new CreateAuthorResult(result.Value.Id, true);
    }
}

public sealed class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}
