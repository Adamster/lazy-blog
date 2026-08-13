using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class RatingSyncRunRepository(LazyBlogDbContext dbContext) : IRatingSyncRunRepository
{
    public Task<DateTime?> GetLastSyncAsync(CancellationToken ct) =>
        dbContext.Set<RatingSyncRun>().Where(run => run.Status == "Succeeded" || run.Status == "Partial")
            .MaxAsync(run => (DateTime?)run.CompletedOnUtc, ct);

    public void Add(RatingSyncRun run) => dbContext.Add(run);
}
