using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IUserBookStatusRepository
{
    Task<UserBookStatus?> GetAsync(Guid userId, Guid bookId, CancellationToken ct);

    Task<IReadOnlyList<UserBookStatus>> GetFilteredAsync(
        string userName,
        ReadingStatus? status,
        string? genre,
        string? search,
        CancellationToken ct);

    Task<IReadOnlyDictionary<Guid, UserBookStatus>> GetEntriesAsync(
        string userName,
        IReadOnlyCollection<Guid> bookIds,
        CancellationToken ct);

    Task<IReadOnlyDictionary<Guid, ReadingStatus>> GetStatusesAsync(
        Guid userId,
        IReadOnlyCollection<Guid> bookIds,
        CancellationToken ct);

    Task<int> GetCountAsync(string userName, ReadingStatus status, CancellationToken ct);

    void Add(UserBookStatus status);

    void Remove(UserBookStatus status);
}
