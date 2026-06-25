using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Arcade.GetMyArcadeStats;

namespace Lazy.Application.Arcade.SubmitScore;

public record SubmitScoreCommand(string Game, int Score) : ICommand<ArcadeStatsResponse>;
