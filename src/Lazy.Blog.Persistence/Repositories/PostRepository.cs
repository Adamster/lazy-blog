using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class PostRepository(LazyBlogDbContext dbContext) : IPostRepository
{
    private const int PostPageSize = 24;

    public async Task<Post?> GetByIdAsync(Guid postId, CancellationToken ct) =>
        await dbContext
            .Set<Post>()
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(post => post.Id == postId, ct);

    public async Task<Post?> GetBySlugAsync(Slug slug, CancellationToken ct) =>
        await dbContext
            .Set<Post>()
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Slug == slug, ct);

    public IQueryable<Post> GetPosts(int offset, CancellationToken ct)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .Include(x => x.Tags);

        return posts;
    }

    public IQueryable<Post> GetPostsByTag(Tag tag, CancellationToken ct)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .AsNoTracking()
            .Include(x => x.Tags)
            .Where(p => p.IsPublished)
            .Where(p => p.Tags.Contains(tag))
            .OrderByDescending(p => p.CreatedOnUtc)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments);

        return posts;
    }

    public void Add(Post post) =>
        dbContext.Set<Post>().Add(post);

    public void Update(Post post) =>
        dbContext.Set<Post>().Update(post);

    public IQueryable<Post> GetPostsByUserId(Guid userId, int offset, CancellationToken ct)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments);

        return posts;
    }

    public IQueryable<Post> GetPostsByUserName(UserName userName, int offset, CancellationToken ct)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .Where(p => p.User.UserName == userName.Value)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Comments);

        return posts;
    }

    public IQueryable<Post> GetPagedPosts(int requestOffset, CancellationToken ct)
    {
        var posts = dbContext.Set<Post>()
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(requestOffset)
            .Take(PostPageSize)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .Include(x => x.Tags);
        return posts;
    }

    public void Delete(Post post) => dbContext.Set<Post>().Remove(post);
}