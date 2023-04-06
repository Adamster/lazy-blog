using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface ICommentRepository
{
    void Add(Comment  comment);

    void Update(Comment comment);

    void Delete(Comment comment);

    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Comment>> GetAllAsync(Guid postId, CancellationToken cancellationToken = default);
}