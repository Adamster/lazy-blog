using Lazy.Domain.Entities.Identity;

namespace Lazy.Domain.Repositories.Identity;

public interface IUserTokenRepository
{
   Task<UserToken?> GetByRefreshTokenAsync(string requestRefreshToken, CancellationToken cancellationToken);

   Task<UserToken?> GetByUserIdAsync(Guid UserId, CancellationToken ct);
   void Update(UserToken storedRefreshToken);
   Task AddAsync(UserToken userToken, CancellationToken cancellationToken);
}