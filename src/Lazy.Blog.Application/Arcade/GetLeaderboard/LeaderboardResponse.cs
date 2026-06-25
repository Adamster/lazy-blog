namespace Lazy.Application.Arcade.GetLeaderboard;

public record LeaderboardResponse(
    string Game,
    IReadOnlyList<LeaderboardEntryResponse> Entries);

public record LeaderboardEntryResponse(
    int Rank,
    string UserName,
    string? AvatarUrl,
    int BestScore,
    int GamesPlayed);
