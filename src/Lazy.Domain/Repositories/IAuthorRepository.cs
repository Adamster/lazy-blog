using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects;

namespace Lazy.Domain.Repositories;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Author?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);

    void Add(Author author);

    void Update(Author author);
}