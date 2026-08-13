using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record RemoveBookStatusCommand(Guid BookId) : ICommand;

public sealed class RemoveBookStatusCommandHandler(IShelfRepository repository, ICurrentUserContext currentUser, IShelfUnitOfWork unitOfWork) : ICommandHandler<RemoveBookStatusCommand>
{
    public async Task<Result> Handle(RemoveBookStatusCommand request, CancellationToken ct)
    {
        var existing = await repository.GetUserBookStatusAsync(currentUser.GetCurrentUserId(), request.BookId, ct);
        if (existing is null) return Result.Success();
        repository.Remove(existing); return await unitOfWork.SaveChangesAsync(null, ct);
    }
}
