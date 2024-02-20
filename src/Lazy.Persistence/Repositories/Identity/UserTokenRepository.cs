using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Repositories.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories.Identity;

public class UserTokenRepository : IUserTokenRepository
{
    private readonly LazyBlogDbContext _dbContext;

    public UserTokenRepository(LazyBlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserToken?> GetByRefreshTokenAsync(string requestRefreshToken, CancellationToken cancellationToken) =>
        await _dbContext.Set<UserToken>()
            .SingleOrDefaultAsync(x => x.Value == requestRefreshToken, cancellationToken);

    public async Task<UserToken?> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        await _dbContext.Set<UserToken>()
            .OrderByDescending(x => x.CreatedOnUtc)
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public void Update(UserToken storedRefreshToken) 
        => _dbContext.Set<UserToken>().Update(storedRefreshToken);

    public async Task AddAsync(UserToken userToken, CancellationToken cancellationToken)
    {
        await _dbContext.Set<UserToken>().AddAsync(userToken, cancellationToken);
    }
}