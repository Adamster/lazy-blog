using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid postId, CancellationToken cancellationToken);

    Task<Post?> GetBySlugAsync(Slug slug, CancellationToken cancellationToken);

    Task<IList<Post>> GetPosts(int offset, CancellationToken cancellationToken);

    void Add(Post post);

    void Update(Post post);

    Task<IList<Post>> GetPostsByUserIdAsync(Guid userId, int offset, CancellationToken cancellationToken);

    Task<IList<Post>> GetPostsByUserNameAsync(UserName userName, int offset, CancellationToken cancellationToken);
}