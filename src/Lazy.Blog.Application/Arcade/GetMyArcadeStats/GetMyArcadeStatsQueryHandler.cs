using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Arcade.Extensions;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Application.Arcade.GetMyArcadeStats;

public class GetMyArcadeStatsQueryHandler(
    IArcadeScoreRepository arcadeScoreRepository,
    ICurrentUserContext currentUserContext)
    : IQueryHandler<GetMyArcadeStatsQuery, ArcadeStatsResponse>
{
    public async Task<Result<ArcadeStatsResponse>> Handle(GetMyArcadeStatsQuery request, CancellationToken ct)
    {
        Result<GameKey> gameKeyResult = GameKey.Create(request.Game);

        if (gameKeyResult.IsFailure)
        {
            return Result.Failure<ArcadeStatsResponse>(gameKeyResult.Error);
        }

        GameKey game = gameKeyResult.Value;

        Guid currentUserId = currentUserContext.GetCurrentUserId();

        ArcadeUserStats stats = await arcadeScoreRepository.GetUserStatsAsync(currentUserId, game, ct);

        return stats.ToArcadeStatsResponse(game);
    }
}
