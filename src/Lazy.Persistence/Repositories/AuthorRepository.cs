using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly LazyBlogDbContext _dbContext;
    
    public AuthorRepository(LazyBlogDbContext dbContext) => 
        _dbContext = dbContext;

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => 
        await _dbContext
            .Set<Author>()
            .FirstOrDefaultAsync(author => author.Id == id, cancellationToken);

    public async Task<Author?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Author>()
            .FirstOrDefaultAsync(author => author.Email == email, cancellationToken);

    public async Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default) =>
        !await _dbContext.Set<Author>()
            .AnyAsync(author => author.Email == email, cancellationToken);

    public void Add(Author author) =>
        _dbContext.Set<Author>().Add(author);

    public void Update(Author author) =>
        _dbContext.Set<Author>().Update(author);
}