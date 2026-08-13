using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IUserBookStatusRepository
{
    Task<UserBookStatus?> GetAsync(Guid userId, Guid bookId, CancellationToken ct);

    Task<Page<UserBookStatus>> GetPagedAsync(
        string userName,
        ReadingStatus status,
        int offset,
        int limit,
        CancellationToken ct);

    Task<IReadOnlyDictionary<Guid, ReadingStatus>> GetStatusesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> bookIds,
        CancellationToken ct);

    void Add(UserBookStatus status);

    void Remove(UserBookStatus status);
}
