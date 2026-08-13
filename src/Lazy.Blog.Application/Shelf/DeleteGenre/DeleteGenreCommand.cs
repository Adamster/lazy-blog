using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteGenreCommand(Guid Id) : ICommand;

public sealed class DeleteGenreCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteGenreCommand>
{
    public async Task<Result> Handle(DeleteGenreCommand request, CancellationToken ct)
    {
        var value = await repository.GetGenreAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.GenreNotFound(request.Id));
        if (await repository.GenreHasBooksAsync(request.Id, ct)) return Result.Failure(DomainErrors.Shelf.GenreInUse);
        repository.Remove(value);
        return await unitOfWork.SaveChangesAsync(ShelfConflict.GenreInUse, ct);
    }
}
