namespace Lazy.Application.Shelf;

public record GenreResponse(Guid Id, string Slug, string Name, int SortOrder, int BookCount);
public record BookGenreResponse(Guid Id, string Slug, string Name);
public record BookAuthorResponse(Guid Id,string Name);
public record BookResponse(Guid Id, string Slug, string Title, BookAuthorResponse Author,
    string? Series, string CoverUrl, decimal Rating, int RatingCount,
    string RatingSource, string? SourceWorkId, BookGenreResponse Genre, string? Status);
public record BooksPageResponse(IReadOnlyList<BookResponse> Items, int Total, int Offset, int Limit,
    DateTime? RatingLastSyncedOnUtc);
public record BookDetailResponse(Guid Id, string Slug, string Title, BookAuthorResponse Author,
    string? Series, IReadOnlyList<string> SeriesTitles, string CoverUrl, decimal Rating, int RatingCount,
    string RatingSource, string? SourceWorkId, string? SourceUrl, DateTime? RatingSyncedOnUtc, BookGenreResponse Genre,
    string? Status, int CommentsCount);
public record BookCommentUserResponse(Guid Id, string DisplayName, string UserName, string? AvatarUrl);
public record BookCommentResponse(Guid Id, BookCommentUserResponse User, string Body,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
public record RatingImportItem(string SourceWorkId, decimal Rating, int RatingCount);
public record RatingSyncResponse(Guid Id, string Source, int BooksSeen, int BooksUpdated, int BooksFailed,
    DateTime CompletedOnUtc);

internal static class ShelfMapping
{
    internal static string StatusName(Domain.Entities.ReadingStatus status) =>
        status == Domain.Entities.ReadingStatus.Read ? "read" : "reading";
}
