using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IBookRepository
{
    Task<Page<Book>> GetPagedAsync(
        string? genre,
        string? search,
        string? sort,
        int offset,
        int limit,
        CancellationToken ct);

    Task<Book?> GetAsync(Guid id, CancellationToken ct);

    Task<Book?> GetBySlugAsync(string slug, CancellationToken ct);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    Task<bool> ConflictsAsync(
        Guid? excludedId,
        string slug,
        string ratingSource,
        string? sourceWorkId,
        CancellationToken ct);

    Task<IReadOnlyList<Book>> GetBySourceIdsAsync(
        string source,
        IReadOnlyCollection<string> sourceWorkIds,
        CancellationToken ct);

    Task<IReadOnlyList<Book>> GetBySeriesIdAsync(Guid seriesId, CancellationToken ct);

    Task<IReadOnlyList<Book>> GetBySeriesIdsAsync(IReadOnlyCollection<Guid> seriesIds, CancellationToken ct);

    Task<IReadOnlyList<Book>> GetFilteredAsync(string? genre, string? authorSlug, string? search, CancellationToken ct);

    Task<bool> HasByAuthorAsync(Guid authorId, CancellationToken ct);

    Task<bool> HasByGenreAsync(Guid genreId, CancellationToken ct);

    Task<bool> HasBySeriesAsync(Guid seriesId, CancellationToken ct);

    Task<IReadOnlyDictionary<Guid, int>> GetCountsByGenreAsync(CancellationToken ct);

    Task<IReadOnlyDictionary<Guid, int>> GetCountsBySeriesAsync(CancellationToken ct);

    void Add(Book book);

    void Remove(Book book);
}
