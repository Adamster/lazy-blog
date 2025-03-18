using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using static Lazy.Persistence.Constants.QueryConstants;

namespace Lazy.Persistence.Repositories;

public class TagRepository : ITagRepository
{
    private readonly LazyBlogDbContext _dbContext;

    public TagRepository(LazyBlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Tag>> SearchTagAsync(string searchTerm, CancellationToken ct)
    {
        return _dbContext.Set<Tag>()
            .AsNoTracking()
            .Where(x => x.Value.Contains(searchTerm))
            .Include(x => x.Posts)
            .ToListAsync(ct);
    }

    public IQueryable<Tag> GetAllTags()
    {
        return _dbContext.Set<Tag>()
            .Include(t => t.Posts)
            .AsNoTracking();

    }

    public Tag? GetTagByValue(string tagValue) =>
        _dbContext
            .Set<Tag>()
            .AsNoTracking()
            .FirstOrDefault(x => x.Value == tagValue);

    public Tag? GetTagById(Guid tagId)
    {
        return _dbContext.Set<Tag>()
            .AsNoTracking()
            .FirstOrDefault(t => t.Id == tagId);
    }

    public Task<List<Tag>> GetTagByIdsAsync(List<Guid> tagIds, CancellationToken ct)
    {
        var tags = _dbContext.Set<Tag>()
            .Where(x => tagIds.Contains(x.Id))
            .ToListAsync(ct);
        return tags;
    }

    public void Update(Tag tag)
    {
        _dbContext.Set<Tag>().Update(tag);
    }

    public void Attach(List<Tag> requestTags)
    {
        foreach (var tag in requestTags)
        {
            _dbContext.Attach(tag);
        }
    }
}