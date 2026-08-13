using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetBookBySlugQuery(string Slug) : IQuery<BookDetailResponse>;

public sealed class GetBookBySlugQueryHandler(IShelfRepository shelf, ICurrentUserContext currentUser) : IQueryHandler<GetBookBySlugQuery, BookDetailResponse>
{
    public async Task<Result<BookDetailResponse>> Handle(GetBookBySlugQuery request, CancellationToken ct)
    {
        var book = await shelf.GetBookBySlugAsync(request.Slug, ct);
        if (book is null) return Result.Failure<BookDetailResponse>(DomainErrors.Shelf.BookSlugNotFound(request.Slug));
        var status = await shelf.GetUserBookStatusAsync(currentUser.GetCurrentUserId(), book.Id, ct);
        return new BookDetailResponse(book.Id, book.Slug, book.Title, new(book.Author.Id, book.Author.Name), ShelfMapping.ToSeriesInfo(book),
            book.CoverUrl, book.Rating, book.RatingCount, book.RatingSource, book.SourceWorkId, book.SourceUrl, book.RatingSyncedOnUtc,
            new(book.Genre.Id, book.Genre.Slug, book.Genre.Name), status is null ? null : ShelfMapping.StatusName(status.Status),
            await shelf.GetBookCommentsCountAsync(book.Id, ct));
    }
}
