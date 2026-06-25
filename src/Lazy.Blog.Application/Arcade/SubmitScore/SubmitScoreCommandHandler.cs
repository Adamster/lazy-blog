using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Arcade.Extensions;
using Lazy.Application.Arcade.GetMyArcadeStats;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Application.Arcade.SubmitScore;

public class SubmitScoreCommandHandler(
    IArcadeScoreRepository arcadeScoreRepository,
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SubmitScoreCommand, ArcadeStatsResponse>
{
    public async Task<Result<ArcadeStatsResponse>> Handle(SubmitScoreCommand request, CancellationToken ct)
    {
        Result<GameKey> gameKeyResult = GameKey.Create(request.Game);

        if (gameKeyResult.IsFailure)
        {
            return Result.Failure<ArcadeStatsResponse>(gameKeyResult.Error);
        }

        Guid currentUserId = currentUserContext.GetCurrentUserId();

        User? user = await userRepository.GetByIdAsync(currentUserId, ct);

        if (user is null)
        {
            return Result.Failure<ArcadeStatsResponse>(DomainErrors.User.NotFound(currentUserId));
        }

        GameKey game = gameKeyResult.Value;

        ArcadeScore score = ArcadeScore.Create(currentUserId, game, request.Score);
        arcadeScoreRepository.Add(score);

        await unitOfWork.SaveChangesAsync(ct);

        ArcadeUserStats stats = await arcadeScoreRepository.GetUserStatsAsync(currentUserId, game, ct);

        return stats.ToArcadeStatsResponse(game);
    }
}
