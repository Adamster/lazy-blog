using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Arcade.GetLeaderboard;

public record GetLeaderboardQuery(string Game, int Take) : IQuery<LeaderboardResponse>;
