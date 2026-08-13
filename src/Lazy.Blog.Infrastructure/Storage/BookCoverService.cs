using Azure;
using Lazy.Application.Abstractions.File;
using Microsoft.Extensions.Logging;

namespace Lazy.Infrastructure.Storage;

public sealed class BookCoverService(
    ICoverImageProcessor imageProcessor,
    ICoverBlobStorage blobStorage,
    ILogger<BookCoverService> logger) : IBookCoverService
{
    public async Task<BookCoverUpload?> ProcessAndUploadAsync(FileUpload file, CancellationToken ct)
    {
        try
        {
            var processed = await imageProcessor.ProcessAsync(file, ct);
            if (processed is null)
            {
                return null;
            }

            string? url = await blobStorage.UploadAsync(processed, ct);
            return url is null
                ? null
                : new BookCoverUpload(
                    url,
                    processed.Sha256,
                    processed.Width,
                    processed.Height,
                    processed.ContentType,
                    file.FileName);
        }
        catch (Exception exception) when (exception is
            ArgumentException or
            InvalidOperationException or
            RequestFailedException or
            NotSupportedException)
        {
            logger.LogWarning(exception, "Invalid or failed Shelf cover {Name}", file.FileName);
            return null;
        }
    }
}
