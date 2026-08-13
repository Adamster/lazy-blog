using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface ISeriesRepository
{
    Task<IReadOnlyList<Series>> GetAllAsync(CancellationToken ct);

    Task<Series?> GetAsync(Guid id, CancellationToken ct);

    Task<Series?> GetBySlugAsync(string slug, CancellationToken ct);

    Task<bool> ConflictsAsync(Guid? excludedId, string slug, string title, CancellationToken ct);

    void Add(Series series);

    void Remove(Series series);
}
