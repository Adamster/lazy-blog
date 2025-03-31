using Azure;
using Lazy.Application.Abstractions.File;
using Microsoft.AspNetCore.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lazy.Infrastructure.Storage;

public class FileService(IOptions<AzureBlobStorageOptions> options, ILogger<FileService> logger)
    : IFileService
{
    private readonly AzureBlobStorageOptions _options = options.Value;

    public async Task<string?> UploadAsync(IFormFile file, Guid fileId, string userName, CancellationToken ct)
    {
        BlobServiceClient blobServiceClient = GetBlobServiceClient();
        BlobContainerClient? blobContainerClient =
            blobServiceClient.GetBlobContainerClient(_options.ImageContainerName);
        if (blobContainerClient is null)
        {
            logger.LogError(
                $"{nameof(blobContainerClient)} is null, error creating blob container client for ${_options.ImageContainerName}");

            return null;
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{fileId}{fileExtension}";

        BlobClient? blobClient = blobContainerClient.GetBlobClient($"{userName}/{fileName}");

        if (blobClient is null)
        {
            logger.LogError($"{nameof(blobClient)} is null, error creating blobClient for {file.FileName}");
            return null;
        }

        try
        {
            await using Stream data = file.OpenReadStream();
            await blobClient.UploadAsync(data, true, ct);

            return blobClient.Uri.AbsoluteUri;
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception,
                $"Upload failed with message: {exception.Message}, error code: {exception.ErrorCode}");
            return null;
        }
    }

    public async Task<bool> DeleteByFilenameAsync(string fileName, string userName, CancellationToken ct)
    {
        var blobServiceClient = GetBlobServiceClient();
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_options.ImageContainerName);

        var deleteResult = await blobContainerClient.DeleteBlobIfExistsAsync($"{userName}/{fileName}", cancellationToken: ct);
        return deleteResult?.Value ?? false;
    }

    public async Task<bool> DeleteByBlobUrl(string requestBlobUrl, string userName, CancellationToken ct)
    {
        var blobUri =  new Uri(requestBlobUrl);
        var blobName = blobUri.Segments[^1];
        return await DeleteByFilenameAsync(blobName, userName, ct);
    }

    private BlobServiceClient GetBlobServiceClient()
    {
        //TODO use keys approach for local development
        BlobServiceClient client = new(
            new Uri($"https://{_options.AccountName}.blob.core.windows.net"),
            new DefaultAzureCredential());

        return client;
    }
}