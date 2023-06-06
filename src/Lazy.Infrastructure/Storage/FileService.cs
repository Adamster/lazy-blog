using Azure;
using Lazy.Application.Abstractions.File;
using Microsoft.AspNetCore.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lazy.Infrastructure.Storage;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly AzureBlobStorageOptions _options;

    public FileService(IOptions<AzureBlobStorageOptions> options, ILogger<FileService> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string?> UploadAsync(IFormFile file, string userName, CancellationToken ct)
    {
        BlobServiceClient blobServiceClient = GetBlobServiceClient();
        BlobContainerClient? blobContainerClient = blobServiceClient.GetBlobContainerClient(_options.ImageContainerName);
        if (blobContainerClient is null)
        {
            _logger.LogError(
                $"{nameof(blobContainerClient)} is null, error creating blob container client for ${_options.ImageContainerName}");

            return null;
        }

        BlobClient? blobClient = blobContainerClient.GetBlobClient($"{userName}/{file.FileName}");
        if (blobClient is null)
        {
            _logger.LogError($"{nameof(blobClient)} is null, error creating blobClient for {file.FileName}");
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
            _logger.LogError(exception,$"Upload failed with message: {exception.Message}, error code: {exception.ErrorCode}");
            return null;
        }
    }

    public Task DeleteAsync(string fileName, CancellationToken ct)
    {
        throw new NotImplementedException();
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