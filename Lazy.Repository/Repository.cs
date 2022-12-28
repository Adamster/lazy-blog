using Lazy.Domain;
using Lazy.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Repository;

public class Repository<T> : IRepository<T> where T : Entity
{
    private readonly DbSet<T> _dbSet;

    public Repository(LazyBlogDbContext lazyBlogDbContext)
    {
        _dbSet = lazyBlogDbContext.Set<T>();
    }

    public async Task<IList<T>> GetItems(int pageIndex = 0, int pageSize = 100)
    {
        return await _dbSet.OrderByDescending(x => x.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
           
            .ToListAsync();
    }

    public async Task<T?> GetItemById(Guid id)
    {
        return await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();
    }
}