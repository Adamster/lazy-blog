using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;
using static Lazy.Persistence.Constants.QueryConstants;

namespace Lazy.Persistence.Repositories;

public class PostRepository(LazyBlogDbContext dbContext) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(Guid postId, CancellationToken ct) =>
        await dbContext
            .Set<Post>()
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(post => post.Id == postId, ct);

    public async Task<Post?> GetBySlugAsync(Slug slug, CancellationToken ct) =>
        await dbContext
            .Set<Post>()
            .Include(p => p.User)
            .Include(p => p.Tags)
            .AsNoTracking()
            .FirstOrDefaultAsync(post => post.Slug == slug, ct);

    public IQueryable<Post> GetPosts(int offset, CancellationToken ct)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedOnUtc)
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .Include(x => x.Tags)
            .Skip(offset)
            .Take(PageSize);

        return posts;
    }

    public IQueryable<Post> GetPostsByTag(Tag tag, int offset, CancellationToken ct)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .AsNoTracking()
            .Include(x => x.Tags)
            .Where(p => p.IsPublished && p.Tags.Contains(tag))
            .OrderByDescending(p => p.PublishedOnUtc)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .Skip(offset)
            .Take(PageSize);
        
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
            .OrderByDescending(p => p.PublishedOnUtc)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .AsSplitQuery()
            .Skip(offset)
            .Take(PageSize);

        return posts;
    }

    public IQueryable<Post> GetPostsByUserName(UserName userName, int offset, CancellationToken ct, bool includeDrafts)
    {
        IQueryable<Post> posts = dbContext.Set<Post>()
            .Where(p => p.User.UserName == userName.Value)
            .OrderByDescending(p => includeDrafts ? p.CreatedOnUtc : p.PublishedOnUtc)
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Comments)
            .Include(p => p.Tags)
            .AsSplitQuery()
            .Skip(offset)
            .Take(PageSize);

        if (!includeDrafts)
        {
            posts = posts.Where(p => p.IsPublished);
        }

        return posts;
    }

    public IQueryable<Post> GetPagedPosts(int requestOffset, CancellationToken ct)
    {
        var posts = dbContext.Set<Post>()
            .AsNoTracking()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedOnUtc)
            .Include(x => x.User)
            .ThenInclude(u => u.PostVotes)
            .Include(x => x.Comments)
            .Include(x => x.Tags)
            .Skip(requestOffset)
            .Take(PageSize);
        return posts;
    }

    public async Task<int> GetPostCountByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var postCount = await dbContext.Set<Post>()
            .Where(p => p.UserId == userId && p.IsPublished)
            .CountAsync(cancellationToken: ct);
        return postCount;
    }
    
    public void Delete(Post post) => dbContext.Set<Post>().Remove(post);
}