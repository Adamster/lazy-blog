using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class BookRepository(LazyBlogDbContext dbContext) : IBookRepository
{
    public async Task<Page<Book>> GetPagedAsync(
        string? genre,
        string? search,
        string? sort,
        int offset,
        int limit,
        CancellationToken ct)
    {
        IQueryable<Book> books = dbContext.Set<Book>()
            .AsNoTracking()
            .Include(book => book.Author)
            .Include(book => book.Genre)
            .Include(book => book.Series);
        if (!string.IsNullOrWhiteSpace(genre))
        {
            books = books.Where(book => book.Genre.Slug == genre);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            books = books.Where(book => book.Title.Contains(term) || book.Author.Name.Contains(term) ||
                (book.Series != null && book.Series.Title.Contains(term)));
        }

        books = sort switch
        {
            "rating" => books.OrderByDescending(book => book.Rating)
                .ThenBy(book => book.Author.Name)
                .ThenBy(book => book.Title),
            "readers" => books.OrderByDescending(book => book.RatingCount)
                .ThenByDescending(book => book.Rating)
                .ThenBy(book => book.Title),
            "author" => books.OrderBy(book => book.Author.Name)
                .ThenBy(book => book.Series != null ? book.Series.Title : book.Title).ThenBy(book => book.Title),
            "title" => books.OrderBy(book => book.Title).ThenBy(book => book.Author.Name),
            _ => books.OrderByDescending(book => (book.Rating * book.RatingCount + 700m) / (book.RatingCount + 100m))
                .ThenByDescending(book => book.RatingCount).ThenBy(book => book.Author.Name).ThenBy(book => book.Title)
        };

        int total = await books.CountAsync(ct);
        IReadOnlyList<Book> items = await books.Skip(offset).Take(limit).ToListAsync(ct);
        return new Page<Book>(items, total);
    }

    public Task<Book?> GetAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Book>().Include(book => book.Author).Include(book => book.Genre).Include(book => book.Series)
            .FirstOrDefaultAsync(book => book.Id == id, ct);

    public Task<Book?> GetBySlugAsync(string slug, CancellationToken ct) =>
        dbContext.Set<Book>()
            .AsNoTracking()
            .Include(book => book.Author)
            .Include(book => book.Genre)
            .Include(book => book.Series)
            .FirstOrDefaultAsync(book => book.Slug == slug, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.Id == id, ct);

    public Task<bool> ConflictsAsync(
        Guid? excludedId,
        string slug,
        string ratingSource,
        string? sourceWorkId,
        CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(
            book => book.Id != excludedId &&
                (book.Slug == slug ||
                 (sourceWorkId != null &&
                  book.RatingSource == ratingSource &&
                  book.SourceWorkId == sourceWorkId)),
            ct);

    public async Task<IReadOnlyList<Book>> GetBySourceIdsAsync(
        string source,
        IReadOnlyCollection<string> sourceWorkIds,
        CancellationToken ct) =>
        await dbContext.Set<Book>()
            .Where(book =>
                book.RatingSource == source &&
                book.SourceWorkId != null &&
                sourceWorkIds.Contains(book.SourceWorkId))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Book>> GetBySeriesIdAsync(Guid seriesId, CancellationToken ct) =>
        await dbContext.Set<Book>().AsNoTracking().Include(book => book.Author).Include(book => book.Genre)
            .Where(book => book.SeriesId == seriesId).OrderBy(book => book.VolumeNumber).ToListAsync(ct);

    public async Task<IReadOnlyList<Book>> GetBySeriesIdsAsync(IReadOnlyCollection<Guid> seriesIds, CancellationToken ct) =>
        await dbContext.Set<Book>().AsNoTracking().Include(book => book.Author).Include(book => book.Genre).Include(book => book.Series)
            .Where(book => book.SeriesId != null && seriesIds.Contains(book.SeriesId.Value))
            .OrderBy(book => book.VolumeNumber).ToListAsync(ct);

    public async Task<IReadOnlyList<Book>> GetFilteredAsync(string? genre, string? authorSlug, string? search, CancellationToken ct)
    {
        IQueryable<Book> books = dbContext.Set<Book>().AsNoTracking()
            .Include(book => book.Author).Include(book => book.Genre).Include(book => book.Series);
        if (!string.IsNullOrWhiteSpace(genre)) books = books.Where(book => book.Genre.Slug == genre);
        if (!string.IsNullOrWhiteSpace(authorSlug)) books = books.Where(book => book.Author.Slug == authorSlug);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            books = books.Where(book => book.Title.Contains(term) || book.Author.Name.Contains(term) ||
                (book.Series != null && book.Series.Title.Contains(term)));
        }

        return await books.ToListAsync(ct);
    }

    public Task<bool> HasByAuthorAsync(Guid authorId, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.AuthorId == authorId, ct);

    public Task<bool> HasByGenreAsync(Guid genreId, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.GenreId == genreId, ct);

    public Task<bool> HasBySeriesAsync(Guid seriesId, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.SeriesId == seriesId, ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetCountsByGenreAsync(CancellationToken ct) =>
        await dbContext.Set<Book>().AsNoTracking().GroupBy(book => book.GenreId)
            .ToDictionaryAsync(group => group.Key, group => group.Count(), ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetCountsBySeriesAsync(CancellationToken ct) =>
        await dbContext.Set<Book>()
            .AsNoTracking()
            .Where(book => book.SeriesId != null)
            .GroupBy(book => book.SeriesId!.Value)
            .ToDictionaryAsync(group => group.Key, group => group.Count(), ct);

    public void Add(Book book) => dbContext.Add(book);

    public void Remove(Book book) => dbContext.Remove(book);
}
