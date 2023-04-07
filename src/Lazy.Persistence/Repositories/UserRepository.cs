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

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => 
        await _dbContext
            .Set<User>()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<User>()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public async Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default) =>
        !await _dbContext.Set<User>()
            .AnyAsync(user => user.Email == email, cancellationToken);

    public void Add(User user) =>
        _dbContext.Set<User>().Add(user);

    public void Update(User user) =>
        _dbContext.Set<User>().Update(user);

    public async Task<User?> GetByUsernameAsync(UserName userName, CancellationToken cancellationToken) =>
        await _dbContext
            .Set<User>()
            .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);

}