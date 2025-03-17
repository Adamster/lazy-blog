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

    public void Update(UserToken storedRefreshToken)
    {
        _dbContext.Set<UserToken>().Attach(storedRefreshToken);
        _dbContext.Set<UserToken>().Update(storedRefreshToken);
    }
    

    public async Task AddAsync(UserToken userToken, CancellationToken cancellationToken)
    {
        await _dbContext.Set<UserToken>().AddAsync(userToken, cancellationToken);
    }

    public async Task<List<UserToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<UserToken>()
            .Where(x => !x.IsUsed && x.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}