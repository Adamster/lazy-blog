using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteSeriesCommand(Guid Id) : ICommand;

public sealed class DeleteSeriesCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteSeriesCommand>
{
    public async Task<Result> Handle(DeleteSeriesCommand request, CancellationToken ct)
    {
        var value = await repository.GetSeriesAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.SeriesNotFound(request.Id));
        if (await repository.SeriesHasBooksAsync(request.Id, ct)) return Result.Failure(DomainErrors.Shelf.SeriesInUse);
        repository.Remove(value);
        return await unitOfWork.SaveChangesAsync(ShelfConflict.SeriesInUse, ct);
    }
}
