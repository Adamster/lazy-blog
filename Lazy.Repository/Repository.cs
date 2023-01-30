using Lazy.Domain;
using Lazy.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Repository;

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly LazyBlogDbContext DbContext;
    private readonly DbSet<T> _dbSet;

    protected Repository(LazyBlogDbContext dbContext)
    {
        DbContext = dbContext;
        _dbSet = dbContext.Set<T>();
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

    public async Task<T> SaveOrUpdate(T item, CancellationToken ct)
    {
        if (item.Id == Guid.Empty)
        {
            await _dbSet.AddAsync(item, ct);
            await DbContext.SaveChangesAsync(ct);
        }

        _dbSet.Attach(item);
        DbContext.Update(item);
         await DbContext.SaveChangesAsync(ct);
         return item;
    }

    public async Task<bool> DeleteItem(Guid id, CancellationToken ct)
    {
        var itemToDelete = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (itemToDelete == null)
        {
            return false;
        }

        _dbSet.Remove(itemToDelete);
        await DbContext.SaveChangesAsync(ct);
        return true;
    }
}