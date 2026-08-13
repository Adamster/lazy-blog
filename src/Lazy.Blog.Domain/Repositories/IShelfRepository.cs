using Lazy.Domain.Entities;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Repositories;

public sealed record Page<T>(IReadOnlyList<T> Items, int Total);

public enum ShelfConflict { Book, Author, Genre, Series, AuthorInUse, GenreInUse, SeriesInUse }

public interface IShelfUnitOfWork
{
    Task<Result> SaveChangesAsync(ShelfConflict? conflict, CancellationToken ct);
}

public interface IShelfRepository
{
    Task<IReadOnlyList<Author>> GetAuthorsAsync(string? query, int limit, CancellationToken ct);
    Task<Author?> GetAuthorAsync(Guid id, CancellationToken ct);
    Task<Author?> GetAuthorByNormalizedNameAsync(string normalizedName, CancellationToken ct);
    Task<bool> AuthorHasBooksAsync(Guid authorId, CancellationToken ct);

    Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken ct);
    Task<Genre?> GetGenreAsync(Guid id, CancellationToken ct);
    Task<bool> GenreConflictsAsync(Guid? excludedId, string slug, string name, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, int>> GetBookCountsByGenreAsync(CancellationToken ct);
    Task<bool> GenreHasBooksAsync(Guid genreId, CancellationToken ct);

    Task<IReadOnlyList<Series>> GetSeriesAsync(CancellationToken ct);
    Task<Series?> GetSeriesAsync(Guid id, CancellationToken ct);
    Task<Series?> GetSeriesBySlugAsync(string slug, CancellationToken ct);
    Task<bool> SeriesConflictsAsync(Guid? excludedId, string slug, string title, CancellationToken ct);
    Task<bool> SeriesHasBooksAsync(Guid seriesId, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, int>> GetBookCountsBySeriesAsync(CancellationToken ct);
    Task<IReadOnlyList<Book>> GetBooksBySeriesIdAsync(Guid seriesId, CancellationToken ct);

    Task<Page<Book>> GetBooksAsync(string? genre, string? search, string? sort, int offset, int limit, CancellationToken ct);
    Task<Book?> GetBookAsync(Guid id, CancellationToken ct);
    Task<Book?> GetBookBySlugAsync(string slug, CancellationToken ct);
    Task<bool> BookExistsAsync(Guid id, CancellationToken ct);
    Task<bool> BookConflictsAsync(Guid? excludedId, string slug, string ratingSource, string? sourceWorkId, CancellationToken ct);
    Task<IReadOnlyList<Book>> GetBooksBySourceIdsAsync(string source, IReadOnlyCollection<string> sourceWorkIds, CancellationToken ct);

    Task<UserBookStatus?> GetUserBookStatusAsync(Guid userId, Guid bookId, CancellationToken ct);
    Task<Page<UserBookStatus>> GetUserBooksAsync(string userName, ReadingStatus status, int offset, int limit, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, ReadingStatus>> GetUserBookStatusesAsync(Guid userId, IReadOnlyCollection<Guid> bookIds, CancellationToken ct);

    Task<BookComment?> GetBookCommentAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<BookComment>> GetBookCommentsAsync(Guid bookId, CancellationToken ct);
    Task<int> GetBookCommentsCountAsync(Guid bookId, CancellationToken ct);

    Task<DateTime?> GetLastRatingSyncAsync(CancellationToken ct);

    void Add(Author author);
    void Add(Genre genre);
    void Add(Series series);
    void Add(Book book);
    void Add(UserBookStatus status);
    void Add(BookComment comment);
    void Add(RatingSyncRun run);

    void Remove(Author author);
    void Remove(Genre genre);
    void Remove(Series series);
    void Remove(Book book);
    void Remove(UserBookStatus status);
    void Remove(BookComment comment);
}
