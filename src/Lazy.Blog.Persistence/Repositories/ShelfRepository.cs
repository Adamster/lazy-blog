using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class ShelfRepository(LazyBlogDbContext dbContext) : IShelfRepository
{
    public async Task<IReadOnlyList<Author>> GetAuthorsAsync(string? query, int limit, CancellationToken ct)
    {
        IQueryable<Author> authors = dbContext.Set<Author>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            authors = authors.Where(author => author.Name.Contains(query.Trim()));
        }

        return await authors.OrderBy(author => author.NormalizedName).Take(limit).ToListAsync(ct);
    }

    public Task<Author?> GetAuthorAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Author>().FirstOrDefaultAsync(author => author.Id == id, ct);

    public Task<Author?> GetAuthorByNormalizedNameAsync(string normalizedName, CancellationToken ct) =>
        dbContext.Set<Author>().FirstOrDefaultAsync(author => author.NormalizedName == normalizedName, ct);

    public Task<bool> AuthorHasBooksAsync(Guid authorId, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.AuthorId == authorId, ct);

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken ct) =>
        await dbContext.Set<Genre>().AsNoTracking().OrderBy(genre => genre.SortOrder).ToListAsync(ct);

    public Task<Genre?> GetGenreAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Genre>().FirstOrDefaultAsync(genre => genre.Id == id, ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetBookCountsByGenreAsync(CancellationToken ct) =>
        await dbContext.Set<Book>().AsNoTracking().GroupBy(book => book.GenreId)
            .ToDictionaryAsync(group => group.Key, group => group.Count(), ct);

    public Task<bool> GenreHasBooksAsync(Guid genreId, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.GenreId == genreId, ct);

    public async Task<Page<Book>> GetBooksAsync(string? genre, string? search, string? sort, int offset, int limit, CancellationToken ct)
    {
        IQueryable<Book> books = dbContext.Set<Book>().AsNoTracking().Include(book => book.Author).Include(book => book.Genre);
        if (!string.IsNullOrWhiteSpace(genre)) books = books.Where(book => book.Genre.Slug == genre);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            books = books.Where(book => book.Title.Contains(term) || book.Author.Name.Contains(term) ||
                (book.Series != null && book.Series.Contains(term)));
        }

        books = sort switch
        {
            "rating" => books.OrderByDescending(book => book.Rating).ThenBy(book => book.Author.Name).ThenBy(book => book.Title),
            "readers" => books.OrderByDescending(book => book.RatingCount).ThenByDescending(book => book.Rating).ThenBy(book => book.Title),
            "author" => books.OrderBy(book => book.Author.Name).ThenBy(book => book.Series ?? book.Title).ThenBy(book => book.Title),
            "title" => books.OrderBy(book => book.Title).ThenBy(book => book.Author.Name),
            _ => books.OrderByDescending(book => (book.Rating * book.RatingCount + 700m) / (book.RatingCount + 100m))
                .ThenByDescending(book => book.RatingCount).ThenBy(book => book.Author.Name).ThenBy(book => book.Title)
        };

        int total = await books.CountAsync(ct);
        IReadOnlyList<Book> items = await books.Skip(offset).Take(limit).ToListAsync(ct);
        return new Page<Book>(items, total);
    }

    public Task<Book?> GetBookAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Book>().Include(book => book.Author).Include(book => book.Genre).FirstOrDefaultAsync(book => book.Id == id, ct);

    public Task<Book?> GetBookBySlugAsync(string slug, CancellationToken ct) =>
        dbContext.Set<Book>().AsNoTracking().Include(book => book.Author).Include(book => book.Genre)
            .FirstOrDefaultAsync(book => book.Slug == slug, ct);

    public Task<bool> BookExistsAsync(Guid id, CancellationToken ct) => dbContext.Set<Book>().AnyAsync(book => book.Id == id, ct);

    public Task<bool> BookConflictsAsync(Guid? excludedId, string slug, string source, string? sourceId, CancellationToken ct) =>
        dbContext.Set<Book>().AnyAsync(book => book.Id != excludedId &&
            (book.Slug == slug || (sourceId != null && book.RatingSource == source && book.SourceWorkId == sourceId)), ct);

    public async Task<IReadOnlyList<Book>> GetBooksBySourceIdsAsync(string source, IReadOnlyCollection<string> ids, CancellationToken ct) =>
        await dbContext.Set<Book>().Where(book => book.RatingSource == source && book.SourceWorkId != null && ids.Contains(book.SourceWorkId)).ToListAsync(ct);

    public Task<UserBookStatus?> GetUserBookStatusAsync(Guid userId, Guid bookId, CancellationToken ct) =>
        dbContext.Set<UserBookStatus>().FirstOrDefaultAsync(status => status.UserId == userId && status.BookId == bookId, ct);

    public async Task<Page<UserBookStatus>> GetUserBooksAsync(string userName, ReadingStatus status, int offset, int limit, CancellationToken ct)
    {
        IQueryable<UserBookStatus> statuses = dbContext.Set<UserBookStatus>().AsNoTracking()
            .Include(value => value.Book).ThenInclude(book => book.Author)
            .Include(value => value.Book).ThenInclude(book => book.Genre)
            .Where(value => value.User.UserName == userName && value.Status == status)
            .OrderByDescending(value => value.UpdatedOnUtc ?? value.CreatedOnUtc);
        int total = await statuses.CountAsync(ct);
        IReadOnlyList<UserBookStatus> items = await statuses.Skip(offset).Take(limit).ToListAsync(ct);
        return new Page<UserBookStatus>(items, total);
    }

    public async Task<IReadOnlyDictionary<Guid, ReadingStatus>> GetUserBookStatusesAsync(Guid userId, IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
        await dbContext.Set<UserBookStatus>().AsNoTracking().Where(status => status.UserId == userId && ids.Contains(status.BookId))
            .ToDictionaryAsync(status => status.BookId, status => status.Status, ct);

    public Task<BookComment?> GetBookCommentAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<BookComment>().FirstOrDefaultAsync(comment => comment.Id == id, ct);

    public async Task<IReadOnlyList<BookComment>> GetBookCommentsAsync(Guid bookId, CancellationToken ct) =>
        await dbContext.Set<BookComment>().AsNoTracking().Include(comment => comment.User).ThenInclude(user => user.Avatar)
            .Where(comment => comment.BookId == bookId).OrderByDescending(comment => comment.CreatedOnUtc).ToListAsync(ct);

    public Task<int> GetBookCommentsCountAsync(Guid bookId, CancellationToken ct) =>
        dbContext.Set<BookComment>().CountAsync(comment => comment.BookId == bookId, ct);

    public Task<DateTime?> GetLastRatingSyncAsync(CancellationToken ct) =>
        dbContext.Set<RatingSyncRun>().Where(run => run.Status == "Succeeded" || run.Status == "Partial")
            .MaxAsync(run => (DateTime?)run.CompletedOnUtc, ct);

    public void Add(Author value) => dbContext.Add(value);
    public void Add(Genre value) => dbContext.Add(value);
    public void Add(Book value) => dbContext.Add(value);
    public void Add(UserBookStatus value) => dbContext.Add(value);
    public void Add(BookComment value) => dbContext.Add(value);
    public void Add(RatingSyncRun value) => dbContext.Add(value);
    public void Remove(Author value) => dbContext.Remove(value);
    public void Remove(Genre value) => dbContext.Remove(value);
    public void Remove(Book value) => dbContext.Remove(value);
    public void Remove(UserBookStatus value) => dbContext.Remove(value);
    public void Remove(BookComment value) => dbContext.Remove(value);
}
