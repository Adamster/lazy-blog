using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record CreateAuthorCommand(string Slug, string Name, string NameEn) : ICommand<CreateAuthorResult>;

public record CreateAuthorResult(Guid Id, bool Created);

public sealed class CreateAuthorCommandHandler(
    IAuthorRepository authors,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<CreateAuthorCommand, CreateAuthorResult>
{
    public async Task<Result<CreateAuthorResult>> Handle(CreateAuthorCommand request, CancellationToken ct)
    {
        var result = Author.Create(Guid.NewGuid(), ShelfInput.Normalize(request.Slug), request.Name, request.NameEn);
        if (result.IsFailure)
        {
            return Result.Failure<CreateAuthorResult>(result.Error);
        }

        var existing = await authors.GetByNormalizedNameAsync(result.Value.NormalizedName, ct);
        if (existing is not null)
        {
            return new CreateAuthorResult(existing.Id, false);
        }

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
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(300);
    }
}
