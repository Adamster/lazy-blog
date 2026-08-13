using System.Text.Json.Serialization;

namespace Lazy.Application.Shelf;

public record GenreResponse(Guid Id, string Slug, string Name, int SortOrder, int BookCount);

public record BookGenreResponse(Guid Id, string Slug, string Name);

public record BookAuthorResponse(Guid Id, string Slug, string Name, string? NameEn);

public record BookSeriesInfo(Guid Id, string Slug, string Title, int VolumeNumber);

public record BookResponse(Guid Id, string Slug, string Title, BookAuthorResponse Author, BookSeriesInfo? Series,
    string CoverUrl, decimal Rating, int RatingCount, string RatingSource, string? SourceWorkId,
    BookGenreResponse Genre, string? Status, string? TitleEn);

public record BooksPageResponse(
    IReadOnlyList<BookResponse> Items,
    int Total,
    int Offset,
    int Limit,
    DateTime? RatingLastSyncedOnUtc);

public record BookDetailResponse(Guid Id, string Slug, string Title, BookAuthorResponse Author, BookSeriesInfo? Series,
    string CoverUrl, decimal Rating, int RatingCount, string RatingSource,
    string? SourceWorkId,
    string? SourceUrl,
    DateTime? RatingSyncedOnUtc,
    BookGenreResponse Genre,
    string? Status,
    int CommentsCount,
    string? TitleEn);

public record BookCommentUserResponse(Guid Id, string DisplayName, string UserName, string? AvatarUrl);

public record BookCommentResponse(
    Guid Id,
    BookCommentUserResponse User,
    string Body,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public record RatingImportItem(string SourceWorkId, decimal Rating, int RatingCount);

public record RatingSyncResponse(
    Guid Id,
    string Source,
    int BooksSeen,
    int BooksUpdated,
    int BooksFailed,
    DateTime CompletedOnUtc);

public record SeriesResponse(Guid Id, string Slug, string Title, int BookCount, string? TitleEn);

public record SeriesDetailResponse(Guid Id, string Slug, string Title, IReadOnlyList<BookResponse> Volumes, string? TitleEn);

public record IdResponse(Guid Id);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ShelfBookCard), "book")]
[JsonDerivedType(typeof(ShelfSeriesCard), "series")]
public abstract record ShelfCatalogItem;

public sealed record ShelfBookCard(
    Guid Id,
    string Slug,
    string Title,
    BookAuthorResponse Author,
    BookGenreResponse Genre,
    string CoverUrl,
    decimal Rating,
    int RatingCount,
    string? Status) : ShelfCatalogItem;

public sealed record ShelfSeriesCard(
    Guid Id,
    string Slug,
    string Title,
    BookAuthorResponse Author,
    IReadOnlyList<string> CoverUrls,
    int BooksCount,
    int ReadCount,
    int ReadingCount,
    decimal Rating,
    int ReadersCount,
    string? Status) : ShelfCatalogItem;

public record ShelfCatalogResponse(
    IReadOnlyList<ShelfCatalogItem> Items,
    int TotalItems,
    int TotalBooks,
    int Offset,
    int Limit,
    DateTime? RatingLastSyncedOnUtc);

internal static class ShelfMapping
{
    internal static string StatusName(Domain.Entities.ReadingStatus status) =>
        status == Domain.Entities.ReadingStatus.Read ? "read" : "reading";

    internal static bool TryStatus(string value, out Domain.Entities.ReadingStatus status)
    {
        bool read = value.Equals("read", StringComparison.OrdinalIgnoreCase);
        bool reading = value.Equals("reading", StringComparison.OrdinalIgnoreCase);
        status = read ? Domain.Entities.ReadingStatus.Read : Domain.Entities.ReadingStatus.Reading;
        return read || reading;
    }

    internal static Domain.Entities.ReadingStatus? LookupStatus(
        IReadOnlyDictionary<Guid, Domain.Entities.ReadingStatus> statuses, Guid bookId) =>
        statuses.TryGetValue(bookId, out var status) ? status : null;

    internal static BookSeriesInfo? ToSeriesInfo(Domain.Entities.Book book) =>
        book.Series is null || book.VolumeNumber is null
            ? null
            : new BookSeriesInfo(book.Series.Id, book.Series.Slug, book.Series.Title, book.VolumeNumber.Value);

    internal static BookResponse ToResponse(
        Domain.Entities.Book book,
        Domain.Entities.ReadingStatus? status) =>
        new(
            book.Id,
            book.Slug,
            book.Title,
            new(book.Author.Id, book.Author.Slug, book.Author.Name, book.Author.NameEn),
            ToSeriesInfo(book),
            book.CoverUrl,
            book.Rating,
            book.RatingCount,
            book.RatingSource,
            book.SourceWorkId,
            new(book.Genre.Id, book.Genre.Slug, book.Genre.Name),
            status is null ? null : StatusName(status.Value),
            book.TitleEn);
}
