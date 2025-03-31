using Microsoft.AspNetCore.Http;

namespace Lazy.Application.Abstractions.File;

public interface IFileService
{
    Task<string?> UploadAsync(IFormFile file, Guid fileId, string userName, CancellationToken ct);

    Task<bool> DeleteByFilenameAsync(string fileName, string userName, CancellationToken ct);
    Task<bool> DeleteByBlobUrl(string requestBlobUrl, string userName, CancellationToken ct);
}