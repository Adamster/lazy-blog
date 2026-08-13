using FluentValidation;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record ImportBookRatingsCommand(
    string Source,
    IReadOnlyList<RatingImportItem> Items) : ICommand<RatingSyncResponse>;

public sealed class ImportBookRatingsCommandHandler(
    IBookRepository books,
    IRatingSyncRunRepository ratingSyncRuns,
    IShelfUnitOfWork unitOfWork) : ICommandHandler<ImportBookRatingsCommand, RatingSyncResponse>
{
    public async Task<Result<RatingSyncResponse>> Handle(ImportBookRatingsCommand request, CancellationToken ct)
    {
        if (request.Items.Select(x => x.SourceWorkId).Distinct(StringComparer.Ordinal).Count() != request.Items.Count)
            return Result.Failure<RatingSyncResponse>(DomainErrors.Shelf.DuplicateRatingSourceWorkId);
        DateTime now = DateTime.UtcNow;
        var run = RatingSyncRun.Create(Guid.NewGuid(), request.Source, now);
        ratingSyncRuns.Add(run);
        var matchingBooks = await books.GetBySourceIdsAsync(
            request.Source,
            request.Items.Select(x => x.SourceWorkId).ToList(),
            ct);
        var lookup = matchingBooks.ToDictionary(x => x.SourceWorkId!);
        int updated = 0;
        int failed = 0;
        foreach (var item in request.Items)
        {
            if (!lookup.TryGetValue(item.SourceWorkId, out var book) ||
                book.UpdateRating(item.Rating, item.RatingCount, request.Source, now).IsFailure)
            {
                failed++;
                continue;
            }
            updated++;
        }
        run.Complete(request.Items.Count, updated, failed, failed == 0 ? null : $"{failed} item(s) failed", now);
        var save = await unitOfWork.SaveChangesAsync(null, ct);
        if (save.IsFailure) return Result.Failure<RatingSyncResponse>(save.Error);
        return new RatingSyncResponse(run.Id, request.Source, request.Items.Count, updated, failed, now);
    }
}

public sealed class ImportBookRatingsCommandValidator : AbstractValidator<ImportBookRatingsCommand>
{
    public ImportBookRatingsCommandValidator()
    {
        RuleFor(x => x.Source).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Items)
            .NotEmpty()
            .Must(items => items
                .Select(item => item.SourceWorkId)
                .Distinct(StringComparer.Ordinal)
                .Count() == items.Count)
            .WithMessage("SourceWorkId values must be unique");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.SourceWorkId).NotEmpty().MaximumLength(100);
            item.RuleFor(x => x.Rating).InclusiveBetween(0, 10);
            item.RuleFor(x => x.RatingCount).GreaterThanOrEqualTo(0);
        });
    }
}
