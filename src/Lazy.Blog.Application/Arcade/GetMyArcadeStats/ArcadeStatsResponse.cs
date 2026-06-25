namespace Lazy.Application.Arcade.GetMyArcadeStats;

public record ArcadeStatsResponse(
    string Game,
    int BestScore,
    int GamesPlayed,
    int? Rank);
