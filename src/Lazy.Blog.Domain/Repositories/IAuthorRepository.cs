using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IAuthorRepository
{
    Task<IReadOnlyList<Author>> GetAllAsync(string? query, int limit, CancellationToken ct);

    Task<Author?> GetAsync(Guid id, CancellationToken ct);

    Task<Author?> GetByNormalizedNameAsync(string normalizedName, CancellationToken ct);

    void Add(Author author);

    void Remove(Author author);
}
