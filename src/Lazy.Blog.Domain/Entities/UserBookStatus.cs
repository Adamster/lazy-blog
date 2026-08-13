using Lazy.Domain.Primitives;

namespace Lazy.Domain.Entities;

public enum ReadingStatus : byte { Reading = 1, Read = 2 }

public sealed class UserBookStatus : IAuditableEntity
{
    private UserBookStatus() { }
    private UserBookStatus(Guid userId, Guid bookId, ReadingStatus status)
        => (UserId, BookId, Status) = (userId, bookId, status);
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid BookId { get; private set; }
    public Book Book { get; private set; } = null!;
    public ReadingStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public static UserBookStatus Create(Guid userId, Guid bookId, ReadingStatus status) => new(userId, bookId, status);
    public void SetStatus(ReadingStatus status) => Status = status;
}
