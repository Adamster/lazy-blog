using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
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

    public IQueryable<PostVote> GetPostVotesByPostId(Guid postId, CancellationToken ct) =>
        _dbContext.Set<PostVote>()
            .AsNoTracking()
            .Where(pv => pv.PostId == postId)
            .OrderBy(pv => pv.CreatedOnUtc);

    public async Task<VoteCounts> GetVoteCountsByAuthorIdAsync(Guid authorId, CancellationToken ct)
    {
        var counts = await _dbContext.Set<PostVote>()
            .AsNoTracking()
            .Where(pv => pv.Post.UserId == authorId)
            .GroupBy(pv => pv.VoteDirection)
            .Select(g => new { Direction = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int upVotes = counts.SingleOrDefault(c => c.Direction == VoteDirection.Up)?.Count ?? 0;
        int downVotes = counts.SingleOrDefault(c => c.Direction == VoteDirection.Down)?.Count ?? 0;

        return new VoteCounts(upVotes, downVotes);
    }

    public void Update(PostVote vote) =>
        _dbContext.Set<PostVote>().Update(vote);
}