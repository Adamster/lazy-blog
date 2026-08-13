using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Application.Shelf;

public record SetBookStatusCommand(Guid BookId, string Status) : ICommand;
public record RemoveBookStatusCommand(Guid BookId) : ICommand;
public record AddBookCommentCommand(Guid BookId, string Body) : ICommand<Guid>;
public record UpdateBookCommentCommand(Guid CommentId, string Body) : ICommand;
public record DeleteBookCommentCommand(Guid CommentId) : ICommand;
public record ImportBookRatingsCommand(string Source, IReadOnlyList<RatingImportItem> Items) : ICommand<RatingSyncResponse>;

public sealed class SetBookStatusCommandHandler(IShelfRepository shelf, ICurrentUserContext currentUser, IUnitOfWork uow)
    : ICommandHandler<SetBookStatusCommand>
{
    public async Task<Result> Handle(SetBookStatusCommand r, CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        if (!await shelf.Books.AnyAsync(x => x.Id == r.BookId, ct)) return Result.Failure(DomainErrors.Shelf.BookNotFound(r.BookId));
        if (!TryParse(r.Status, out var status)) return Result.Failure(DomainErrors.Shelf.InvalidStatus);
        var existing = await shelf.UserBookStatuses.FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == r.BookId, ct);
        if (existing is null) shelf.Add(UserBookStatus.Create(userId, r.BookId, status)); else existing.SetStatus(status);
        await uow.SaveChangesAsync(ct); return Result.Success();
    }
    private static bool TryParse(string value, out ReadingStatus status)
    { status = value.Equals("read", StringComparison.OrdinalIgnoreCase) ? ReadingStatus.Read : ReadingStatus.Reading;
      return value.Equals("read", StringComparison.OrdinalIgnoreCase) || value.Equals("reading", StringComparison.OrdinalIgnoreCase); }
}

public sealed class RemoveBookStatusCommandHandler(IShelfRepository shelf, ICurrentUserContext currentUser, IUnitOfWork uow)
    : ICommandHandler<RemoveBookStatusCommand>
{
    public async Task<Result> Handle(RemoveBookStatusCommand r, CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        var existing = await shelf.UserBookStatuses.FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == r.BookId, ct);
        if (existing is not null) { shelf.Remove(existing); await uow.SaveChangesAsync(ct); }
        return Result.Success();
    }
}

public sealed class AddBookCommentCommandHandler(IShelfRepository shelf, ICurrentUserContext currentUser, IUnitOfWork uow)
    : ICommandHandler<AddBookCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddBookCommentCommand r, CancellationToken ct)
    {
        if (!await shelf.Books.AnyAsync(x => x.Id == r.BookId, ct)) return Result.Failure<Guid>(DomainErrors.Shelf.BookNotFound(r.BookId));
        var commentResult = BookComment.Create(Guid.NewGuid(), r.BookId, currentUser.GetCurrentUserId(), r.Body.Trim());
        if(commentResult.IsFailure)return Result.Failure<Guid>(commentResult.Error);var comment=commentResult.Value;shelf.Add(comment); await uow.SaveChangesAsync(ct); return comment.Id;
    }
}

public sealed class UpdateBookCommentCommandHandler(IShelfRepository shelf, ICurrentUserContext currentUser, IUnitOfWork uow)
    : ICommandHandler<UpdateBookCommentCommand>
{
    public async Task<Result> Handle(UpdateBookCommentCommand r, CancellationToken ct)
    {
        var comment = await shelf.BookComments.FirstOrDefaultAsync(x => x.Id == r.CommentId, ct);
        if (comment is null) return Result.Failure(DomainErrors.Shelf.CommentNotFound(r.CommentId));
        if (comment.UserId != currentUser.GetCurrentUserId()) return Result.Failure(DomainErrors.Shelf.UnauthorizedCommentUpdate);
        var update=comment.Update(r.Body.Trim());if(update.IsFailure)return update;await uow.SaveChangesAsync(ct); return Result.Success();
    }
}

public sealed class DeleteBookCommentCommandHandler(IShelfRepository shelf, ICurrentUserContext currentUser, IUnitOfWork uow)
    : ICommandHandler<DeleteBookCommentCommand>
{
    public async Task<Result> Handle(DeleteBookCommentCommand r, CancellationToken ct)
    {
        var comment = await shelf.BookComments.FirstOrDefaultAsync(x => x.Id == r.CommentId, ct);
        if (comment is null) return Result.Failure(DomainErrors.Shelf.CommentNotFound(r.CommentId));
        if (comment.UserId != currentUser.GetCurrentUserId()) return Result.Failure(DomainErrors.Shelf.UnauthorizedCommentUpdate);
        shelf.Remove(comment); await uow.SaveChangesAsync(ct); return Result.Success();
    }
}

public sealed class ImportBookRatingsCommandHandler(IShelfRepository shelf, IUnitOfWork uow)
    : ICommandHandler<ImportBookRatingsCommand, RatingSyncResponse>
{
    public async Task<Result<RatingSyncResponse>> Handle(ImportBookRatingsCommand r, CancellationToken ct)
    {
        if (r.Items.Select(x => x.SourceWorkId).Distinct(StringComparer.Ordinal).Count() != r.Items.Count)
            return Result.Failure<RatingSyncResponse>(DomainErrors.Shelf.DuplicateRatingSourceWorkId);
        var now = DateTime.UtcNow;
        var run = RatingSyncRun.Create(Guid.NewGuid(), r.Source, now); shelf.Add(run);
        var ids = r.Items.Select(x => x.SourceWorkId).Distinct().ToList();
        var books = await shelf.Books.Where(x => x.RatingSource == r.Source && x.SourceWorkId != null && ids.Contains(x.SourceWorkId)).ToListAsync(ct);
        var lookup = books.ToDictionary(x => x.SourceWorkId!); var updated = 0; var failed = 0;
        foreach (var item in r.Items)
        {
            if (!lookup.TryGetValue(item.SourceWorkId, out var book)) { failed++; continue; }
            book.UpdateRating(item.Rating, item.RatingCount, r.Source, now); updated++;
        }
        var error = failed == 0 ? null : $"{failed} source work id(s) were not found";
        run.Complete(r.Items.Count, updated, failed, error, now); await uow.SaveChangesAsync(ct);
        return new RatingSyncResponse(run.Id, r.Source, r.Items.Count, updated, failed, now);
    }
}
