using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class AuthorRepository(LazyBlogDbContext dbContext) : IAuthorRepository
{
    public async Task<IReadOnlyList<Author>> GetAllAsync(string? query, int limit, CancellationToken ct)
    {
        IQueryable<Author> authors = dbContext.Set<Author>().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            authors = authors.Where(author => author.Name.Contains(query.Trim()));
        }

        return await authors.OrderBy(author => author.NormalizedName).Take(limit).ToListAsync(ct);
    }

    public Task<Author?> GetAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Author>().FirstOrDefaultAsync(author => author.Id == id, ct);

    public Task<Author?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct) =>
        dbContext.Set<Author>().FirstOrDefaultAsync(author => author.NormalizedName == normalizedName, ct);

    public void Add(Author author) => dbContext.Add(author);

    public void Remove(Author author) => dbContext.Remove(author);
}
