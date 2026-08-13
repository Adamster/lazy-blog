using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteGenreCommand(Guid Id) : ICommand;

public sealed class DeleteGenreCommandHandler(
    IGenreRepository genres,
    IBookRepository books,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteGenreCommand>
{
    public async Task<Result> Handle(DeleteGenreCommand request, CancellationToken ct)
    {
        var value = await genres.GetAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.GenreNotFound(request.Id));
        if (await books.HasByGenreAsync(request.Id, ct)) return Result.Failure(DomainErrors.Shelf.GenreInUse);
        genres.Remove(value);
        return await unitOfWork.SaveChangesAsync(ShelfConflict.GenreInUse, ct);
    }
}
