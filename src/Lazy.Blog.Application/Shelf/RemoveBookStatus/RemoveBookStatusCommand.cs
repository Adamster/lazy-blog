using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record RemoveBookStatusCommand(Guid BookId) : ICommand;

public sealed class RemoveBookStatusCommandHandler(
    IUserBookStatusRepository userBookStatuses,
    ICurrentUserContext currentUser,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<RemoveBookStatusCommand>
{
    public async Task<Result> Handle(RemoveBookStatusCommand request, CancellationToken ct)
    {
        var existing = await userBookStatuses.GetAsync(currentUser.GetCurrentUserId(), request.BookId, ct);
        if (existing is null) return Result.Success();
        userBookStatuses.Remove(existing);
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}
