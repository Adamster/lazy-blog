using System.Data;
using Lazy.Domain.Primitives;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lazy.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly LazyBlogDbContext _dbContext;

    public UnitOfWork(LazyBlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        UpdateAuditableEntities();

        return _dbContext.SaveChangesAsync(ct);
    }

    public IDbTransaction BeginTransaction()
    {
        var transaction = _dbContext.Database.BeginTransaction();

        return transaction.GetDbTransaction();
    }

    private void UpdateAuditableEntities()
    {
        IEnumerable<EntityEntry<IAuditableEntity>> entries =
            _dbContext
                .ChangeTracker
                .Entries<IAuditableEntity>();

        foreach (EntityEntry<IAuditableEntity> entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(a => a.CreatedOnUtc)
                    .CurrentValue = DateTime.UtcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(a => a.UpdatedOnUtc)
                    .CurrentValue = DateTime.UtcNow;
            }
        }
    }
}