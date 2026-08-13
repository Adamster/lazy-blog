using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteSeriesCommand(Guid Id) : ICommand;

public sealed class DeleteSeriesCommandHandler(
    ISeriesRepository series,
    IBookRepository books,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteSeriesCommand>
{
    public async Task<Result> Handle(DeleteSeriesCommand request, CancellationToken ct)
    {
        var value = await series.GetAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.SeriesNotFound(request.Id));
        if (await books.HasBySeriesAsync(request.Id, ct)) return Result.Failure(DomainErrors.Shelf.SeriesInUse);
        series.Remove(value);
        return await unitOfWork.SaveChangesAsync(ShelfConflict.SeriesInUse, ct);
    }
}
