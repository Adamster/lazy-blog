using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly LazyBlogDbContext _dbContext;

    public CommentRepository(LazyBlogDbContext dbContext) =>
        _dbContext = dbContext;

    public void Add(Comment comment) =>
        _dbContext.Set<Comment>().Add(comment);

    public void Update(Comment comment) =>
        _dbContext.Set<Comment>().Update(comment);

    public void Delete(Comment comment) =>
        _dbContext.Set<Comment>().Remove(comment);

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Comment>()
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    
    public async Task<List<Comment>> GetAllAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        List<Comment> comments = await _dbContext.Set<Comment>()
            .OrderByDescending(x => x.CreatedOnUtc)
            .Where(c => c.PostId == postId)
            .Include(x => x.User)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return comments;
    }
}