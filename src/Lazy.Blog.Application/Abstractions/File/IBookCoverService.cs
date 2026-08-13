using Microsoft.AspNetCore.Http;
namespace Lazy.Application.Abstractions.File;
public record BookCoverUpload(string Url, string Sha256, int Width, int Height, string ContentType, string OriginalFileName);
public interface IBookCoverService
{
    Task<BookCoverUpload?> ProcessAndUploadAsync(IFormFile file, CancellationToken ct);
    Task<bool> DeleteAsync(string url, CancellationToken ct);
}
