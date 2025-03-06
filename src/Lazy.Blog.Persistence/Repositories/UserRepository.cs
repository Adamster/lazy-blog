using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LazyBlogDbContext _dbContext;
    
    public UserRepository(LazyBlogDbContext dbContext) => 
        _dbContext = dbContext;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => 
        await _dbContext
            .Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, ct);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default) =>
        await _dbContext
            .Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email.Value, ct);

    public async Task<bool> IsEmailUniqueAsync(Email email, CancellationToken ct = default) =>
        !await _dbContext.Set<User>()
            .AsNoTracking()
            .AnyAsync(user => user.Email == email.Value, ct);

    public void Add(User user) =>
        _dbContext.Set<User>().Add(user);

    public void Update(User user)
    {
        _dbContext.Set<User>().Update(user);
    }

    public async Task<User?> GetByUsernameAsync(UserName userName, CancellationToken ct) =>
        await _dbContext
            .Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserName == userName.Value, ct);

}