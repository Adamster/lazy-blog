using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteBookCommand(Guid Id) : ICommand;

public sealed class DeleteBookCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteBookCommand>
{
    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken ct)
    {
        var value = await repository.GetBookAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.BookNotFound(request.Id));
        repository.Remove(value);
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}
