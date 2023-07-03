namespace Lazy.Infrastructure.Storage;

public class AzureBlobStorageOptions
{
    public string AccountName { get; init; } = null!;
    public string ImageContainerName { get; init; } = null!;
}