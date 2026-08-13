using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteBookCommand(Guid Id) : ICommand;

public sealed class DeleteBookCommandHandler(
    IBookRepository books,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteBookCommand>
{
    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken ct)
    {
        var value = await books.GetAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.BookNotFound(request.Id));
        books.Remove(value);
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}
