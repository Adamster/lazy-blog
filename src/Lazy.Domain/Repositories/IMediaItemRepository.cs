using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IMediaItemRepository
{
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task Add(MediaItem item);

    void Delete(MediaItem item);
}