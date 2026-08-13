using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteAuthorCommand(Guid Id) : ICommand;

public sealed class DeleteAuthorCommandHandler(IShelfRepository repository, IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteAuthorCommand>
{
    public async Task<Result> Handle(DeleteAuthorCommand request, CancellationToken ct)
    {
        var value = await repository.GetAuthorAsync(request.Id, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.AuthorNotFound(request.Id));
        if (await repository.AuthorHasBooksAsync(request.Id, ct)) return Result.Failure(DomainErrors.Shelf.AuthorInUse);
        repository.Remove(value);
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}
