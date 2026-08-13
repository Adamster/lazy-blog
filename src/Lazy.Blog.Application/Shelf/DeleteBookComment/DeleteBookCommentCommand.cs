using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record DeleteBookCommentCommand(Guid CommentId) : ICommand;

public sealed class DeleteBookCommentCommandHandler(IShelfRepository repository, ICurrentUserContext currentUser, IShelfUnitOfWork unitOfWork) : ICommandHandler<DeleteBookCommentCommand>
{
    public async Task<Result> Handle(DeleteBookCommentCommand request, CancellationToken ct)
    {
        var value = await repository.GetBookCommentAsync(request.CommentId, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.CommentNotFound(request.CommentId));
        if (value.UserId != currentUser.GetCurrentUserId()) return Result.Failure(DomainErrors.Shelf.UnauthorizedCommentUpdate);
        if (!value.CanEdit(DateTime.UtcNow)) return Result.Failure(DomainErrors.Shelf.CommentEditWindowExpired);
        repository.Remove(value);
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}
