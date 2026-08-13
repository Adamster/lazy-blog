using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IGenreRepository
{
    Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken ct);

    Task<Genre?> GetAsync(Guid id, CancellationToken ct);

    Task<bool> ConflictsAsync(Guid? excludedId, string slug, string name, CancellationToken ct);

    void Add(Genre genre);

    void Remove(Genre genre);
}
