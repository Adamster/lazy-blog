using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IRatingSyncRunRepository
{
    Task<DateTime?> GetLastSyncAsync(CancellationToken ct);

    void Add(RatingSyncRun run);
}
