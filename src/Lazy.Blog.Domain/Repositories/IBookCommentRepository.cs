using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IBookCommentRepository
{
    Task<BookComment?> GetAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<BookComment>> GetByBookIdAsync(Guid bookId, CancellationToken ct);

    Task<int> GetCountAsync(Guid bookId, CancellationToken ct);

    void Add(BookComment comment);

    void Remove(BookComment comment);
}
