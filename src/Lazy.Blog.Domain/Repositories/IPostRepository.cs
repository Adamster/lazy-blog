using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid postId, CancellationToken ct);

    Task<Post?> GetBySlugAsync(Slug slug, CancellationToken ct);

    IQueryable<Post> GetPosts(int offset, CancellationToken ct);

    IQueryable<Post> GetPostsByTag(Tag tag, CancellationToken ct);

    void Add(Post post);

    void Update(Post post);

    void Delete(Post post);

    IQueryable<Post> GetPostsByUserId(Guid userId, int offset, CancellationToken ct);

    IQueryable<Post> GetPostsByUserName(UserName userName, int offset, CancellationToken ct);
    IQueryable<Post> GetPagedPosts(int requestOffset, CancellationToken ct);
}