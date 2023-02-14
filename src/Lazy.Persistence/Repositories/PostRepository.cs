using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private readonly LazyBlogDbContext _dbContext;

    public PostRepository(LazyBlogDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Post?> GetByIdAsync(Guid postId, CancellationToken cancellationToken) =>
        await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(post => post.Id == postId, cancellationToken);

    public void Add(Post post) =>
        _dbContext.Set<Post>().Add(post);

    public void Update(Post post) =>
        _dbContext.Set<Post>().Update(post);
}