﻿using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private const int PostPageSize = 10;

    private readonly LazyBlogDbContext _dbContext;
    
    public PostRepository(LazyBlogDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<Post?> GetByIdAsync(Guid postId, CancellationToken cancellationToken) =>
        await _dbContext
            .Set<Post>()
            .FirstOrDefaultAsync(post => post.Id == postId, cancellationToken);

    public async Task<IList<Post>> GetPosts(int offset, CancellationToken cancellationToken)
    {
        List<Post> posts = await _dbContext.Set<Post>()
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(offset)
            .Take(PostPageSize)
            .ToListAsync(cancellationToken);

        return posts;
    }

    public void Add(Post post) =>
        _dbContext.Set<Post>().Add(post);

    public void Update(Post post) =>
        _dbContext.Set<Post>().Update(post);
}