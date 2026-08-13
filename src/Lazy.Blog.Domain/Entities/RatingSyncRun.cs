using Lazy.Domain.Primitives;

namespace Lazy.Domain.Entities;

public sealed class RatingSyncRun : Entity
{
    private RatingSyncRun() { }
    private RatingSyncRun(Guid id, string source, DateTime startedOnUtc) : base(id)
        => (Source, StartedOnUtc, Status) = (source, startedOnUtc, "Running");
    public string Source { get; private set; } = null!;
    public DateTime StartedOnUtc { get; private set; }
    public DateTime? CompletedOnUtc { get; private set; }
    public string Status { get; private set; } = null!;
    public int BooksSeen { get; private set; }
    public int BooksUpdated { get; private set; }
    public int BooksFailed { get; private set; }
    public string? ErrorSummary { get; private set; }
    public static RatingSyncRun Create(Guid id, string source, DateTime startedOnUtc) => new(id, source, startedOnUtc);
    public void Complete(int seen, int updated, int failed, string? error, DateTime completed)
        => (BooksSeen, BooksUpdated, BooksFailed, ErrorSummary, CompletedOnUtc, Status) =
            (seen, updated, failed, error, completed, failed == 0 ? "Succeeded" : "Partial");
}
