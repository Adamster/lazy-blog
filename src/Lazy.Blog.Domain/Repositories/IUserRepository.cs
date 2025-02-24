using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);

    Task<bool> IsEmailUniqueAsync(Email email, CancellationToken ct = default);

    void Add(User user);

    void Update(User user);
    Task<User?> GetByUsernameAsync(UserName userName, CancellationToken ct);
}