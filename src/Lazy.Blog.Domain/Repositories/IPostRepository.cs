using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid postId, CancellationToken ct);

    Task<Post?> GetBySlugAsync(Slug slug, CancellationToken ct);

    Task<IList<Post>> GetPostsAsync(int offset, CancellationToken ct);

    Task<IList<Post>> GetPostsByTagAsync(Tag tag, CancellationToken ct);

    void Add(Post post);

    void Update(Post post);

    void Delete(Post post);

    Task<IList<Post>> GetPostsByUserIdAsync(Guid userId, int offset, CancellationToken ct);

    Task<IList<Post>> GetPostsByUserNameAsync(UserName userName, int offset, CancellationToken ct);
    IQueryable<Post> GetIQueryablePostsAsync(int requestOffset, CancellationToken ct);
}