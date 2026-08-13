using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record UpdateSeriesCommand(Guid Id, string Slug, string Title) : ICommand;

public sealed class UpdateSeriesCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<UpdateSeriesCommand>
{
    public async Task<Result> Handle(UpdateSeriesCommand request, CancellationToken ct)
    {
        var value = await repository.GetSeriesAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.SeriesNotFound(request.Id));
        string slug = ShelfInput.Normalize(request.Slug);
        string title = ShelfInput.Normalize(request.Title);
        if (await repository.SeriesConflictsAsync(request.Id, slug, title, ct))
            return Result.Failure(DomainErrors.Shelf.SeriesAlreadyExists);
        var result = value.Update(slug, title);
        if (result.IsFailure) return result;
        return await unitOfWork.SaveChangesAsync(ShelfConflict.Series, ct);
    }
}

public sealed class UpdateSeriesCommandValidator : AbstractValidator<UpdateSeriesCommand>
{
    public UpdateSeriesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
    }
}
