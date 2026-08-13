namespace Lazy.Application.Abstractions.File;

public sealed record FileUpload(string FileName, string ContentType, long Length, Func<Stream> OpenReadStream);
public sealed record BookCoverUpload(string Url, string Sha256, int Width, int Height, string ContentType, string OriginalFileName);

public interface IBookCoverService
{
    // Covers are immutable, content-addressed assets. Request handlers never delete them because
    // multiple books or concurrent requests may reference the same blob. Retention/GC is an
    // operational concern and must only delete blobs proven unreferenced from a consistent snapshot.
    Task<BookCoverUpload?> ProcessAndUploadAsync(FileUpload file, CancellationToken ct);
}
