namespace Lazy.Domain.Repositories;

public sealed record LeaderboardEntry(
    Guid UserId,
    string UserName,
    string? AvatarUrl,
    int BestScore,
    int GamesPlayed);
