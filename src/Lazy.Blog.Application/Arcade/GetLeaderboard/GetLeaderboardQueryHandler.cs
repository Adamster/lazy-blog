using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Arcade.Extensions;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Application.Arcade.GetLeaderboard;

public class GetLeaderboardQueryHandler(IArcadeScoreRepository arcadeScoreRepository)
    : IQueryHandler<GetLeaderboardQuery, LeaderboardResponse>
{
    private const int DefaultTake = 10;
    private const int MaxTake = 100;

    public async Task<Result<LeaderboardResponse>> Handle(GetLeaderboardQuery request, CancellationToken ct)
    {
        Result<GameKey> gameKeyResult = GameKey.Create(request.Game);

        if (gameKeyResult.IsFailure)
        {
            return Result.Failure<LeaderboardResponse>(gameKeyResult.Error);
        }

        GameKey game = gameKeyResult.Value;

        int take = request.Take <= 0 ? DefaultTake : Math.Min(request.Take, MaxTake);

        IReadOnlyList<LeaderboardEntry> entries =
            await arcadeScoreRepository.GetLeaderboardAsync(game, take, ct);

        return new LeaderboardResponse(game.Value, entries.ToLeaderboardResponse());
    }
}
