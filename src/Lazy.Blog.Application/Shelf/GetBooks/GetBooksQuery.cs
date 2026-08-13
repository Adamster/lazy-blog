using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetBooksQuery(
    string? Genre,
    string? Search,
    string? Sort,
    int Offset,
    int Limit) : IQuery<BooksPageResponse>;

public sealed class GetBooksQueryValidator : AbstractValidator<GetBooksQuery>
{
    public GetBooksQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}

public sealed class GetBooksQueryHandler(
    IBookRepository books,
    IUserBookStatusRepository userBookStatuses,
    IRatingSyncRunRepository ratingSyncRuns,
    ICurrentUserContext currentUser) : IQueryHandler<GetBooksQuery, BooksPageResponse>
{
    public async Task<Result<BooksPageResponse>> Handle(GetBooksQuery request, CancellationToken ct)
    {
        int offset = Math.Max(0, request.Offset);
        int limit = Math.Clamp(request.Limit, 1, 100);
        var page = await books.GetPagedAsync(request.Genre, request.Search, request.Sort, offset, limit, ct);
        var statuses = await userBookStatuses.GetStatusesAsync(
            currentUser.GetCurrentUserId(),
            page.Items.Select(x => x.Id).ToList(),
            ct);
        var items = page.Items
            .Select(book => ShelfMapping.ToResponse(book, ShelfMapping.LookupStatus(statuses, book.Id)))
            .ToList();
        return new BooksPageResponse(
            items,
            page.Total,
            offset,
            limit,
            await ratingSyncRuns.GetLastSyncAsync(ct));
    }
}
