using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private const int PostPageSize = 50;

    private readonly LazyBlogDbContext _dbContext;
    
    public PostRepository(LazyBlogDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Post?> GetByIdAsync(Guid postId, CancellationToken cancellationToken) =>
        await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Id == postId, cancellationToken);

    public async Task<Post?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken) =>
        await _dbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(post => post.Slug == slug, cancellationToken);

    public async Task<IList<Post>> GetPostsAsync(int offset, CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        return posts;
    }

    public async Task<IList<Post>> GetPostsByTagAsync(Tag tag, CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        return posts;
    }

    public void Add(Post post) =>
        _dbContext.Set<Post>().Add(post);

    public void Update(Post post) =>
        _dbContext.Set<Post>().Update(post);

    public async Task<IList<Post>> GetPostsByUserIdAsync(Guid userId, int offset, CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        return posts;
    }

    public async Task<IList<Post>> GetPostsByUserNameAsync(UserName userName, int offset, CancellationToken cancellationToken)
    {
        List<Post> posts = await _dbContext.Set<Post>()
            .Where(p => p.User.UserName == userName)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Comments)
            .ToListAsync(cancellationToken);

        return posts;   
    }

    public void Delete(Post post) => _dbContext.Set<Post>().Remove(post);
}