namespace Lazy.Domain.Repositories;

public readonly record struct ArcadeUserStats(int BestScore, int GamesPlayed, int? Rank);
