using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class MediaItemRepository : IMediaItemRepository
{
    private readonly LazyBlogDbContext _dbContext;

    public MediaItemRepository(LazyBlogDbContext dbContext) => _dbContext = dbContext;

    public async Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
       return await _dbContext.Set<MediaItem>()
            .AsNoTracking()
            .FirstOrDefaultAsync(mi => mi.Id == id, cancellationToken: ct);
    }

    public async Task Add(MediaItem item) => await _dbContext.Set<MediaItem>().AddAsync(item);

    public void Delete(MediaItem item) => _dbContext.Set<MediaItem>().Remove(item);
    public Task<MediaItem?> GetByUrlAsync(string requestBlobUrl, CancellationToken cancellationToken)
    {
      return _dbContext.Set<MediaItem>()
          .FirstOrDefaultAsync(mi => mi.UploadedUrl == requestBlobUrl, cancellationToken: cancellationToken);
    }

    public Task<List<MediaItem>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<MediaItem>()
            .Where(mi => mi.UserId == userId)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}