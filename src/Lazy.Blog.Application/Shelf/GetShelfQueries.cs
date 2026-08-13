using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Application.Shelf;

public record GetGenresQuery : IQuery<IReadOnlyList<GenreResponse>>;
public record GetBooksQuery(string? Genre, string? Search, string? Sort, int Offset, int Limit) : IQuery<BooksPageResponse>;
public record GetBookBySlugQuery(string Slug) : IQuery<BookDetailResponse>;
public record GetUserBooksQuery(string UserName, string Status, int Offset, int Limit) : IQuery<BooksPageResponse>;
public record GetBookCommentsQuery(Guid BookId) : IQuery<IReadOnlyList<BookCommentResponse>>;
public record GetBookAuthorsQuery(string? Query) : IQuery<IReadOnlyList<BookAuthorResponse>>;

public sealed class GetBookAuthorsQueryHandler(IShelfRepository shelf):IQueryHandler<GetBookAuthorsQuery,IReadOnlyList<BookAuthorResponse>>
{
    public async Task<Result<IReadOnlyList<BookAuthorResponse>>> Handle(GetBookAuthorsQuery r,CancellationToken ct)
    {
        var query=r.Query?.Trim();IReadOnlyList<BookAuthorResponse> values=await shelf.Authors
            .Where(x=>string.IsNullOrWhiteSpace(query)||x.Name.Contains(query)).OrderBy(x=>x.NormalizedName).Take(10)
            .Select(x=>new BookAuthorResponse(x.Id,x.Name)).ToListAsync(ct);
        return Result.Success(values);
    }
}

public sealed class GetGenresQueryHandler(IShelfRepository shelf) : IQueryHandler<GetGenresQuery, IReadOnlyList<GenreResponse>>
{
    public async Task<Result<IReadOnlyList<GenreResponse>>> Handle(GetGenresQuery request, CancellationToken ct) =>
        await shelf.Genres.OrderBy(x => x.SortOrder)
            .Select(x => new GenreResponse(x.Id, x.Slug, x.Name, x.SortOrder,
                shelf.Books.Count(b => b.GenreId == x.Id))).ToListAsync(ct);
}

public sealed class GetBooksQueryHandler(IShelfRepository shelf, ICurrentUserContext currentUser) : IQueryHandler<GetBooksQuery, BooksPageResponse>
{
    public async Task<Result<BooksPageResponse>> Handle(GetBooksQuery r, CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        var q = shelf.Books;
        if (!string.IsNullOrWhiteSpace(r.Genre)) q = q.Where(x => x.Genre.Slug == r.Genre);
        if (!string.IsNullOrWhiteSpace(r.Search))
        {
            var search = r.Search.Trim();
            q = q.Where(x => x.Title.Contains(search) || x.Author.Name.Contains(search) || (x.Series != null && x.Series.Contains(search)));
        }
        q = r.Sort switch
        {
            "rating" => q.OrderByDescending(x => x.Rating).ThenBy(x => x.Author.Name).ThenBy(x => x.Title),
            "readers" => q.OrderByDescending(x => x.RatingCount).ThenByDescending(x => x.Rating).ThenBy(x => x.Title),
            "author" => q.OrderBy(x => x.Author.Name).ThenBy(x => x.Series ?? x.Title).ThenBy(x => x.Title),
            "title" => q.OrderBy(x => x.Title).ThenBy(x => x.Author.Name),
            _ => q.OrderByDescending(x => (x.Rating * x.RatingCount + 700m) / (x.RatingCount + 100m))
                .ThenByDescending(x => x.RatingCount).ThenBy(x => x.Author.Name).ThenBy(x => x.Title)
        };
        var total = await q.CountAsync(ct);
        var items = await q.Skip(Math.Max(0, r.Offset)).Take(Math.Clamp(r.Limit, 1, 100))
            .Select(x => new BookResponse(x.Id, x.Slug, x.Title, new BookAuthorResponse(x.Author.Id,x.Author.Name),
                x.Series, x.CoverUrl, x.Rating, x.RatingCount, x.RatingSource, x.SourceWorkId,
                new BookGenreResponse(x.Genre.Id, x.Genre.Slug, x.Genre.Name),
                shelf.UserBookStatuses.Where(s => s.UserId == userId && s.BookId == x.Id)
                    .Select(s => s.Status == ReadingStatus.Read ? "read" : "reading").FirstOrDefault()))
            .ToListAsync(ct);
        var synced = await shelf.RatingSyncRuns.Where(x => x.Status == "Succeeded" || x.Status == "Partial")
            .MaxAsync(x => (DateTime?)x.CompletedOnUtc, ct);
        return new BooksPageResponse(items, total, Math.Max(0, r.Offset), Math.Clamp(r.Limit, 1, 100), synced);
    }
}

public sealed class GetBookBySlugQueryHandler(IShelfRepository shelf, ICurrentUserContext currentUser) : IQueryHandler<GetBookBySlugQuery, BookDetailResponse>
{
    public async Task<Result<BookDetailResponse>> Handle(GetBookBySlugQuery r, CancellationToken ct)
    {
        var userId = currentUser.GetCurrentUserId();
        var raw = await shelf.Books.Where(x => x.Slug == r.Slug)
            .Select(x => new { x.Id, x.Slug, x.Title, Author=new BookAuthorResponse(x.Author.Id,x.Author.Name),
                x.Series, x.SeriesTitlesJson, x.CoverUrl, x.Rating, x.RatingCount, x.RatingSource, x.SourceWorkId, x.SourceUrl, x.RatingSyncedOnUtc,
                Genre = new BookGenreResponse(x.Genre.Id, x.Genre.Slug, x.Genre.Name),
                Status = shelf.UserBookStatuses.Where(s => s.UserId == userId && s.BookId == x.Id)
                    .Select(s => s.Status == ReadingStatus.Read ? "read" : "reading").FirstOrDefault(),
                CommentsCount = shelf.BookComments.Count(c => c.BookId == x.Id) }).FirstOrDefaultAsync(ct);
        if (raw is null) return Result.Failure<BookDetailResponse>(DomainErrors.Shelf.BookSlugNotFound(r.Slug));
        var titles = string.IsNullOrWhiteSpace(raw.SeriesTitlesJson) ? [] :
            JsonSerializer.Deserialize<List<string>>(raw.SeriesTitlesJson) ?? [];
        var book = new BookDetailResponse(raw.Id, raw.Slug, raw.Title, raw.Author,
            raw.Series, titles, raw.CoverUrl, raw.Rating, raw.RatingCount, raw.RatingSource, raw.SourceWorkId, raw.SourceUrl,
            raw.RatingSyncedOnUtc, raw.Genre, raw.Status, raw.CommentsCount);
        return Result.Success(book);
    }
}

public sealed class GetUserBooksQueryHandler(IShelfRepository shelf) : IQueryHandler<GetUserBooksQuery, BooksPageResponse>
{
    public async Task<Result<BooksPageResponse>> Handle(GetUserBooksQuery r, CancellationToken ct)
    {
        if (!TryStatus(r.Status, out var status)) return Result.Failure<BooksPageResponse>(DomainErrors.Shelf.InvalidStatus);
        var q = shelf.UserBookStatuses.Where(x => x.User.UserName == r.UserName && x.Status == status)
            .OrderByDescending(x => x.UpdatedOnUtc ?? x.CreatedOnUtc);
        var total = await q.CountAsync(ct);
        var items = await q.Skip(Math.Max(0, r.Offset)).Take(Math.Clamp(r.Limit, 1, 100)).Select(x => new BookResponse(
            x.Book.Id, x.Book.Slug, x.Book.Title, new BookAuthorResponse(x.Book.Author.Id,x.Book.Author.Name),
            x.Book.Series, x.Book.CoverUrl, x.Book.Rating, x.Book.RatingCount, x.Book.RatingSource, x.Book.SourceWorkId,
            new BookGenreResponse(x.Book.Genre.Id, x.Book.Genre.Slug, x.Book.Genre.Name), r.Status.ToLower())).ToListAsync(ct);
        var synced = await shelf.RatingSyncRuns.Where(x => x.Status == "Succeeded" || x.Status == "Partial")
            .MaxAsync(x => (DateTime?)x.CompletedOnUtc, ct);
        return new BooksPageResponse(items, total, Math.Max(0, r.Offset), Math.Clamp(r.Limit, 1, 100), synced);
    }
    private static bool TryStatus(string value, out ReadingStatus status)
    { status = value.Equals("read", StringComparison.OrdinalIgnoreCase) ? ReadingStatus.Read : ReadingStatus.Reading;
      return value.Equals("read", StringComparison.OrdinalIgnoreCase) || value.Equals("reading", StringComparison.OrdinalIgnoreCase); }
}

public sealed class GetBookCommentsQueryHandler(IShelfRepository shelf) : IQueryHandler<GetBookCommentsQuery, IReadOnlyList<BookCommentResponse>>
{
    public async Task<Result<IReadOnlyList<BookCommentResponse>>> Handle(GetBookCommentsQuery r, CancellationToken ct)
    {
        if (!await shelf.Books.AnyAsync(x => x.Id == r.BookId, ct)) return Result.Failure<IReadOnlyList<BookCommentResponse>>(DomainErrors.Shelf.BookNotFound(r.BookId));
        IReadOnlyList<BookCommentResponse> comments = await shelf.BookComments.Where(x => x.BookId == r.BookId).OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new BookCommentResponse(x.Id, new BookCommentUserResponse(x.UserId, x.User.DisplayName.Value,
                x.User.UserName!, x.User.Avatar == null ? null : x.User.Avatar.Url), x.Body, x.CreatedOnUtc, x.UpdatedOnUtc)).ToListAsync(ct);
        return Result.Success(comments);
    }
}
