using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private const int PostPageSize = 24;

    private readonly LazyBlogDbContext _dbContext;
    
    public PostRepository(LazyBlogDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Post?> GetByIdAsync(Guid postId, CancellationToken ct) =>
        await _dbContext
            .Set<Post>()
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(post => post.Id == postId, ct);

    public async Task<Post?> GetBySlugAsync(Slug slug, CancellationToken ct) =>
        await _dbContext
            .Set<Post>()
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Slug == slug, ct);

    public async Task<IList<Post>> GetPostsAsync(int offset, CancellationToken ct)
    {
        List<Post> posts = await _dbContext.Set<Post>()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .Include(x => x.Tags)
            .ToListAsync(ct);

        return posts;
    }

    public async Task<IList<Post>> GetPostsByTagAsync(Tag tag, CancellationToken ct)
    {
        List<Post> posts = await _dbContext.Set<Post>()
            .AsNoTracking()
            .Include(x => x.Tags)
            .Where(p => p.IsPublished)
            .Where(p => p.Tags.Contains(tag))
            .OrderByDescending(p => p.CreatedOnUtc)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .ToListAsync(ct);

        return posts;
    }

    public void Add(Post post) =>
        _dbContext.Set<Post>().Add(post);

    public void Update(Post post) =>
        _dbContext.Set<Post>().Update(post);

    public async Task<IList<Post>> GetPostsByUserIdAsync(Guid userId, int offset, CancellationToken ct)
    {
        List<Post> posts = await _dbContext.Set<Post>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .ToListAsync(ct);

        return posts;
    }

    public async Task<IList<Post>> GetPostsByUserNameAsync(UserName userName, int offset, CancellationToken ct)
    {
        List<Post> posts = await _dbContext.Set<Post>()
            .Where(p => p.User.UserName == userName)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Comments)
            .ToListAsync(ct);

        return posts;   
    }

    public void Delete(Post post) => _dbContext.Set<Post>().Remove(post);
}