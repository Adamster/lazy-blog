using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class PostVoteRepository : IPostVoteRepository
{
    private readonly LazyBlogDbContext _dbContext;

    public PostVoteRepository(LazyBlogDbContext dbContext)
        => _dbContext = dbContext;


    public void Add(PostVote vote) =>
        _dbContext.Set<PostVote>().Add(vote);

    public void Delete(PostVote vote) =>
        _dbContext.Set<PostVote>().Remove(vote);

    public async Task<PostVote?> GetPostVoteForUserIdAsync(
        Guid userId, 
        Guid postId,
        CancellationToken ct)
    {
        return await _dbContext.Set<PostVote>()
            .AsNoTracking()
            .Where(pv => pv.PostId == postId)
            .Include(pv => pv.Post)
            .Include(pv => pv.User)
            .SingleOrDefaultAsync(p => p.UserId == userId, ct);
    }

    public void Update(PostVote vote) =>
        _dbContext.Set<PostVote>().Update(vote);
}