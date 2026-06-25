using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Domain.Repositories;

public interface IArcadeScoreRepository
{
    void Add(ArcadeScore score);

    Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(GameKey game, int take, CancellationToken ct);

    Task<ArcadeUserStats> GetUserStatsAsync(Guid userId, GameKey game, CancellationToken ct);
}
