using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;
using Microsoft.EntityFrameworkCore;
using static Lazy.Persistence.Constants.QueryConstants;

namespace Lazy.Persistence.Repositories;

public class PostRepository(
    LazyBlogDbContext dbContext,
    IDbContextFactory<LazyBlogDbContext> dbContextFactory) : IPostRepository
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
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var postCount = await ctx.Set<Post>()
            .Where(p => p.UserId == userId && p.IsPublished)
            .CountAsync(cancellationToken: ct);
        return postCount;
    }

    public async Task<int> GetTotalViewsByUserIdAsync(Guid userId, CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        long totalViews = await ctx.Set<Post>()
            .Where(p => p.UserId == userId)
            .SumAsync(p => (long?)p.Views, ct) ?? 0;
        return (int)totalViews;
    }

    public async Task<IReadOnlyList<MonthlyPostCount>> GetMonthlyPostCountsByUserIdAsync(Guid userId, CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var monthlyPostCounts = await ctx.Set<Post>()
            .Where(p => p.UserId == userId && p.IsPublished)
            .GroupBy(p => new { p.CreatedOnUtc.Year, p.CreatedOnUtc.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyPostCount(g.Key.Year, g.Key.Month, g.Count()))
            .ToListAsync(ct);
        return monthlyPostCounts;
    }

    public async Task<IReadOnlyList<MonthlyPostCount>> GetMonthlyPostCountsAsync(CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var monthlyPostCounts = await ctx.Set<Post>()
            .Where(p => p.IsPublished)
            .GroupBy(p => new { p.CreatedOnUtc.Year, p.CreatedOnUtc.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyPostCount(g.Key.Year, g.Key.Month, g.Count()))
            .ToListAsync(ct);
        return monthlyPostCounts;
    }

    public async Task<MonthlyTopAuthor?> GetMostActiveAuthorAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var topAuthor = await ctx.Set<Post>()
            .Where(p => p.IsPublished && p.CreatedOnUtc >= fromUtc && p.CreatedOnUtc <= toUtc)
            .GroupBy(p => p.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                PostCount = g.Count(),
                NetRating = g.Sum(p => p.Rating)
            })
            .OrderByDescending(g => g.PostCount)
            .ThenByDescending(g => g.NetRating)
            .FirstOrDefaultAsync(ct);

        if (topAuthor is null)
        {
            return null;
        }

        var user = await ctx.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == topAuthor.UserId, ct);

        if (user is null)
        {
            return null;
        }

        return new MonthlyTopAuthor(user, topAuthor.PostCount, topAuthor.NetRating);
    }

    public async Task<MonthlyTopPost?> GetMostViewedPostAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

        var topPost = await ctx.Set<Post>()
            .Where(p => p.IsPublished && p.CreatedOnUtc >= fromUtc && p.CreatedOnUtc <= toUtc)
            .OrderByDescending(p => p.Views)
            .Select(p => new MonthlyTopPost(
                p.Title.Value,
                p.Slug.Value,
                p.User.UserName!,
                p.Views,
                p.Rating))
            .FirstOrDefaultAsync(ct);

        return topPost;
    }

    public void Delete(Post post) => dbContext.Set<Post>().Remove(post);
}