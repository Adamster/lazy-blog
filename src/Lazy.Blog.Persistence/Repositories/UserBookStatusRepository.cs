using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class UserBookStatusRepository(LazyBlogDbContext dbContext) : IUserBookStatusRepository
{
    public Task<UserBookStatus?> GetAsync(Guid userId, Guid bookId, CancellationToken ct) =>
        dbContext.Set<UserBookStatus>()
            .FirstOrDefaultAsync(status => status.UserId == userId && status.BookId == bookId, ct);

    public async Task<IReadOnlyList<UserBookStatus>> GetFilteredAsync(
        string userName,
        ReadingStatus? status,
        string? genre,
        string? search,
        CancellationToken ct)
    {
        string normalizedUserName = userName.ToUpperInvariant();
        IQueryable<UserBookStatus> statuses = dbContext.Set<UserBookStatus>().AsNoTracking()
            .Include(value => value.Book).ThenInclude(book => book.Author)
            .Include(value => value.Book).ThenInclude(book => book.Genre)
            .Include(value => value.Book).ThenInclude(book => book.Series)
            .Where(value => value.User.NormalizedUserName == normalizedUserName);
        if (status is not null) statuses = statuses.Where(value => value.Status == status);
        if (!string.IsNullOrWhiteSpace(genre)) statuses = statuses.Where(value => value.Book.Genre.Slug == genre);
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            statuses = statuses.Where(value => value.Book.Title.Contains(term) || value.Book.Author.Name.Contains(term) ||
                (value.Book.Series != null && value.Book.Series.Title.Contains(term)));
        }

        return await statuses.ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<Guid, UserBookStatus>> GetEntriesAsync(
        string userName,
        IReadOnlyCollection<Guid> bookIds,
        CancellationToken ct)
    {
        string normalizedUserName = userName.ToUpperInvariant();
        return await dbContext.Set<UserBookStatus>()
            .AsNoTracking()
            .Where(value => value.User.NormalizedUserName == normalizedUserName && bookIds.Contains(value.BookId))
            .ToDictionaryAsync(value => value.BookId, value => value, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, ReadingStatus>> GetStatusesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> bookIds,
        CancellationToken ct) =>
        await dbContext.Set<UserBookStatus>()
            .AsNoTracking()
            .Where(status => status.UserId == userId && bookIds.Contains(status.BookId))
            .ToDictionaryAsync(status => status.BookId, status => status.Status, ct);

    public Task<int> GetCountAsync(string userName, ReadingStatus status, CancellationToken ct)
    {
        string normalizedUserName = userName.ToUpperInvariant();
        return dbContext.Set<UserBookStatus>().AsNoTracking()
            .CountAsync(value => value.User.NormalizedUserName == normalizedUserName && value.Status == status, ct);
    }

    public void Add(UserBookStatus status) => dbContext.Add(status);

    public void Remove(UserBookStatus status) => dbContext.Remove(status);
}
