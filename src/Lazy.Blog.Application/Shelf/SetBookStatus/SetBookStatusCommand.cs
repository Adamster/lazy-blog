using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record SetBookStatusCommand(Guid BookId, string Status) : ICommand;

public sealed class SetBookStatusCommandHandler(IShelfRepository repository, ICurrentUserContext currentUser, IShelfUnitOfWork unitOfWork) : ICommandHandler<SetBookStatusCommand>
{
    public async Task<Result> Handle(SetBookStatusCommand request, CancellationToken ct)
    {
        if (!await repository.BookExistsAsync(request.BookId, ct)) return Result.Failure(DomainErrors.Shelf.BookNotFound(request.BookId));
        if (!ShelfMapping.TryStatus(request.Status, out var status)) return Result.Failure(DomainErrors.Shelf.InvalidStatus);
        Guid userId = currentUser.GetCurrentUserId();
        var existing = await repository.GetUserBookStatusAsync(userId, request.BookId, ct);
        if (existing is null) repository.Add(UserBookStatus.Create(userId, request.BookId, status)); else existing.SetStatus(status);
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}

public sealed class SetBookStatusCommandValidator : AbstractValidator<SetBookStatusCommand>
{
    public SetBookStatusCommandValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.Status).Must(x => ShelfMapping.TryStatus(x, out _));
    }
}
