using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Lazy.Infrastructure.Storage;

public interface ICoverBlobStorage
{
    Task<string?> UploadAsync(ProcessedCover cover, CancellationToken ct);
}

public sealed class CoverBlobStorage : ICoverBlobStorage
{
    private readonly BlobContainerClient _container;

    public CoverBlobStorage(IOptions<AzureBlobStorageOptions> options)
    {
        AzureBlobStorageOptions value = options.Value;
        var service = new BlobServiceClient(
            new Uri($"https://{value.AccountName}.blob.core.windows.net"),
            new DefaultAzureCredential());
        _container = service.GetBlobContainerClient(value.ImageContainerName);
    }

    public async Task<string?> UploadAsync(ProcessedCover cover, CancellationToken ct)
    {
        string path = $"shelf/covers/{cover.Sha256[..16]}.{cover.Extension}";
        BlobClient blob = _container.GetBlobClient(path);
        if ((await blob.ExistsAsync(ct)).Value)
        {
            BlobProperties properties = (await blob.GetPropertiesAsync(cancellationToken: ct)).Value;
            return properties.Metadata.TryGetValue("sha256", out string? storedHash) && storedHash == cover.Sha256
                ? blob.Uri.AbsoluteUri
                : null;
        }

        await using var upload = new MemoryStream(cover.Bytes);
        await blob.UploadAsync(
            upload,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = cover.ContentType },
                Metadata = new Dictionary<string, string> { ["sha256"] = cover.Sha256 },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            },
            ct);
        return blob.Uri.AbsoluteUri;
    }
}
