using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Repositories;
using Lazy.Domain.Repositories.Identity;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.Logout;

public class LogoutCommandHandler(
    IUserTokenRepository userTokenRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        UserToken? storedRefreshToken =
            await userTokenRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (storedRefreshToken is null ||
            storedRefreshToken.UserId != currentUserContext.GetCurrentUserId())
        {
            return Result.Success();
        }

        storedRefreshToken.Invalidate();
        userTokenRepository.Update(storedRefreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
