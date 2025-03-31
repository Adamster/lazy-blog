using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public class MediaItemRepository(LazyBlogDbContext dbContext) : IMediaItemRepository
{
    public async Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await dbContext.Set<MediaItem>()
            .AsNoTracking()
            .FirstOrDefaultAsync(mi => mi.Id == id, cancellationToken: ct);

    public async Task Add(MediaItem item) =>
        await dbContext.Set<MediaItem>().AddAsync(item);

    public void Delete(MediaItem item) =>
        dbContext.Set<MediaItem>().Remove(item);

    public Task<MediaItem?> GetByUrlAsync(string requestBlobUrl, CancellationToken cancellationToken)
        => dbContext.Set<MediaItem>()
            .FirstOrDefaultAsync(mi => mi.UploadedUrl == requestBlobUrl, cancellationToken);

    public Task<List<MediaItem>> GetByUserId(Guid userId, CancellationToken cancellationToken)
        => dbContext.Set<MediaItem>()
            .Where(mi => mi.UserId == userId)
            .ToListAsync(cancellationToken: cancellationToken);
}