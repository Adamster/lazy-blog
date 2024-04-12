using Lazy.Domain.Entities.Identity;

namespace Lazy.Domain.Repositories.Identity;

public interface IUserTokenRepository
{
   Task<UserToken?> GetByRefreshTokenAsync(string requestRefreshToken, CancellationToken cancellationToken);
   void Update(UserToken storedRefreshToken);
   Task AddAsync(UserToken userToken, CancellationToken cancellationToken);
}