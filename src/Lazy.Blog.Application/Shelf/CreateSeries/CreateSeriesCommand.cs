using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record CreateSeriesCommand(string Slug, string Title) : ICommand<Guid>;

public sealed class CreateSeriesCommandHandler(
    ISeriesRepository series,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<CreateSeriesCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateSeriesCommand request, CancellationToken ct)
    {
        string slug = ShelfInput.Normalize(request.Slug);
        string title = ShelfInput.Normalize(request.Title);
        if (await series.ConflictsAsync(null, slug, title, ct))
            return Result.Failure<Guid>(DomainErrors.Shelf.SeriesAlreadyExists);
        var result = Series.Create(Guid.NewGuid(), slug, title);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        series.Add(result.Value);
        var saveResult = await unitOfWork.SaveChangesAsync(ShelfConflict.Series, ct);
        return saveResult.IsFailure ? Result.Failure<Guid>(saveResult.Error) : result.Value.Id;
    }
}

public sealed class CreateSeriesCommandValidator : AbstractValidator<CreateSeriesCommand>
{
    public CreateSeriesCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
    }
}
