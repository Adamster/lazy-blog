using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid postId, CancellationToken cancellationToken);

    void Add(Post post);

    void Update(Post post);
}