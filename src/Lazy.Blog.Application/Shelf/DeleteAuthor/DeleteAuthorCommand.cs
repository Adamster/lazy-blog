using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteAuthorCommand(Guid Id) : ICommand;

public sealed class DeleteAuthorCommandHandler(
    IAuthorRepository authors,
    IBookRepository books,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteAuthorCommand>
{
    public async Task<Result> Handle(DeleteAuthorCommand request, CancellationToken ct)
    {
        var value = await authors.GetAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.AuthorNotFound(request.Id));
        if (await books.HasByAuthorAsync(request.Id, ct)) return Result.Failure(DomainErrors.Shelf.AuthorInUse);
        authors.Remove(value);
        return await unitOfWork.SaveChangesAsync(ShelfConflict.AuthorInUse, ct);
    }
}
