using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.File;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record AddBookCommentCommand(Guid BookId, string Body) : ICommand<Guid>;

public sealed class AddBookCommentCommandHandler(
    IBookRepository books,
    IBookCommentRepository comments,
    ICurrentUserContext currentUser,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<AddBookCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddBookCommentCommand request, CancellationToken ct)
    {
        if (!await books.ExistsAsync(request.BookId, ct))
        {
            return Result.Failure<Guid>(DomainErrors.Shelf.BookNotFound(request.BookId));
        }

        var result = BookComment.Create(
            Guid.NewGuid(),
            request.BookId,
            currentUser.GetCurrentUserId(),
            request.Body.Trim());
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }
        comments.Add(result.Value);
        var save = await unitOfWork.SaveChangesAsync(null, ct);
        return save.IsFailure ? Result.Failure<Guid>(save.Error) : result.Value.Id;
    }
}

public sealed class AddBookCommentCommandValidator : AbstractValidator<AddBookCommentCommand>
{
    public AddBookCommentCommandValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(BookComment.MaxLength);
    }
}
