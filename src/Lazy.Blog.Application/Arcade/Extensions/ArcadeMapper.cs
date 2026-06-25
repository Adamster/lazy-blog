using Lazy.Application.Arcade.GetLeaderboard;
using Lazy.Application.Arcade.GetMyArcadeStats;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Application.Arcade.Extensions;

public static class ArcadeMapper
{
    public static ArcadeStatsResponse ToArcadeStatsResponse(this ArcadeUserStats stats, GameKey game) =>
        new(game.Value, stats.BestScore, stats.GamesPlayed, stats.Rank);

    public static IReadOnlyList<LeaderboardEntryResponse> ToLeaderboardResponse(
        this IReadOnlyList<LeaderboardEntry> entries) =>
        entries
            .Select((entry, index) => new LeaderboardEntryResponse(
                index + 1,
                entry.UserName,
                entry.AvatarUrl,
                entry.BestScore,
                entry.GamesPlayed))
            .ToList();
}
