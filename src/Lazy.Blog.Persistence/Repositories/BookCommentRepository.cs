using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class BookCommentRepository(LazyBlogDbContext dbContext) : IBookCommentRepository
{
    public Task<BookComment?> GetAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<BookComment>().FirstOrDefaultAsync(comment => comment.Id == id, ct);

    public async Task<IReadOnlyList<BookComment>> GetByBookIdAsync(Guid bookId, CancellationToken ct) =>
        await dbContext.Set<BookComment>()
            .AsNoTracking()
            .Include(comment => comment.User)
            .ThenInclude(user => user.Avatar)
            .Where(comment => comment.BookId == bookId)
            .OrderByDescending(comment => comment.CreatedOnUtc)
            .ToListAsync(ct);

    public Task<int> GetCountAsync(Guid bookId, CancellationToken ct) =>
        dbContext.Set<BookComment>().CountAsync(comment => comment.BookId == bookId, ct);

    public void Add(BookComment comment) => dbContext.Add(comment);

    public void Remove(BookComment comment) => dbContext.Remove(comment);
}
