using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Arcade;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class ArcadeScoreRepository(
    LazyBlogDbContext dbContext,
    IDbContextFactory<LazyBlogDbContext> dbContextFactory) : IArcadeScoreRepository
{
    public void Add(ArcadeScore score) =>
        dbContext.Set<ArcadeScore>().Add(score);

    public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(GameKey game, int take, CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var leaderboard = await ctx.Set<ArcadeScore>()
            .AsNoTracking()
            .Where(s => s.Game == game)
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                BestScore = g.Max(s => s.Score),
                GamesPlayed = g.Count()
            })
            .OrderByDescending(g => g.BestScore)
            .ThenBy(g => g.GamesPlayed)
            .Take(take)
            .Join(
                ctx.Set<User>().AsNoTracking(),
                g => g.UserId,
                u => u.Id,
                (g, u) => new LeaderboardEntry(
                    u.Id,
                    u.UserName!,
                    u.Avatar != null ? u.Avatar.Url : null,
                    g.BestScore,
                    g.GamesPlayed))
            .ToListAsync(ct);

        return leaderboard;
    }

    public async Task<ArcadeUserStats> GetUserStatsAsync(Guid userId, GameKey game, CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var stats = await ctx.Set<ArcadeScore>()
            .AsNoTracking()
            .Where(s => s.Game == game && s.UserId == userId)
            .GroupBy(s => s.UserId)
            .Select(g => new { BestScore = g.Max(s => s.Score), GamesPlayed = g.Count() })
            .FirstOrDefaultAsync(ct);

        if (stats is null)
        {
            return new ArcadeUserStats(0, 0, null);
        }

        int playersAhead = await ctx.Set<ArcadeScore>()
            .AsNoTracking()
            .Where(s => s.Game == game)
            .GroupBy(s => s.UserId)
            .Select(g => g.Max(s => s.Score))
            .CountAsync(best => best > stats.BestScore, ct);

        return new ArcadeUserStats(stats.BestScore, stats.GamesPlayed, playersAhead + 1);
    }
}
