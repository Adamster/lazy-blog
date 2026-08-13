using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class UserBookStatusRepository(LazyBlogDbContext dbContext) : IUserBookStatusRepository
{
    public Task<UserBookStatus?> GetAsync(Guid userId, Guid bookId, CancellationToken ct) =>
        dbContext.Set<UserBookStatus>()
            .FirstOrDefaultAsync(status => status.UserId == userId && status.BookId == bookId, ct);

    public async Task<Page<UserBookStatus>> GetPagedAsync(
        string userName,
        ReadingStatus status,
        int offset,
        int limit,
        CancellationToken ct)
    {
        string normalizedUserName = userName.ToUpperInvariant();
        IQueryable<UserBookStatus> statuses = dbContext.Set<UserBookStatus>().AsNoTracking()
            .Include(value => value.Book).ThenInclude(book => book.Author)
            .Include(value => value.Book).ThenInclude(book => book.Genre)
            .Include(value => value.Book).ThenInclude(book => book.Series)
            .Where(value => value.User.NormalizedUserName == normalizedUserName && value.Status == status)
            .OrderByDescending(value => value.UpdatedOnUtc ?? value.CreatedOnUtc);
        int total = await statuses.CountAsync(ct);
        IReadOnlyList<UserBookStatus> items = await statuses.Skip(offset).Take(limit).ToListAsync(ct);
        return new Page<UserBookStatus>(items, total);
    }

    public async Task<IReadOnlyDictionary<Guid, ReadingStatus>> GetStatusesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> bookIds,
        CancellationToken ct) =>
        await dbContext.Set<UserBookStatus>()
            .AsNoTracking()
            .Where(status => status.UserId == userId && bookIds.Contains(status.BookId))
            .ToDictionaryAsync(status => status.BookId, status => status.Status, ct);

    public void Add(UserBookStatus status) => dbContext.Add(status);

    public void Remove(UserBookStatus status) => dbContext.Remove(status);
}
