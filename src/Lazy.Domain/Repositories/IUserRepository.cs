using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);

    void Add(User user);

    void Update(User user);
    Task<User?> GetByUsernameAsync(UserName userName, CancellationToken cancellationToken);
}