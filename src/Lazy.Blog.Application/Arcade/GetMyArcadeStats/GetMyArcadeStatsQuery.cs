using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Arcade.GetMyArcadeStats;

public record GetMyArcadeStatsQuery(string Game) : IQuery<ArcadeStatsResponse>;
