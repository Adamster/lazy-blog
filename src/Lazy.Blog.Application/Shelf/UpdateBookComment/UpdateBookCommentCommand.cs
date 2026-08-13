using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record UpdateBookCommentCommand(Guid CommentId, string Body) : ICommand;

public sealed class UpdateBookCommentCommandHandler(
    IBookCommentRepository comments,
    ICurrentUserContext currentUser,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<UpdateBookCommentCommand>
{
    public async Task<Result> Handle(UpdateBookCommentCommand request, CancellationToken ct)
    {
        var value = await comments.GetAsync(request.CommentId, ct);
        if (value is null) return Result.Failure(DomainErrors.Shelf.CommentNotFound(request.CommentId));
        if (value.UserId != currentUser.GetCurrentUserId())
        {
            return Result.Failure(DomainErrors.Shelf.UnauthorizedCommentUpdate);
        }
        if (!value.CanEdit(DateTime.UtcNow)) return Result.Failure(DomainErrors.Shelf.CommentEditWindowExpired);
        var result = value.Update(request.Body.Trim());
        if (result.IsFailure)
        {
            return result;
        }
        return await unitOfWork.SaveChangesAsync(null, ct);
    }
}

public sealed class UpdateBookCommentCommandValidator : AbstractValidator<UpdateBookCommentCommand>
{
    public UpdateBookCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(BookComment.MaxLength);
    }
}
