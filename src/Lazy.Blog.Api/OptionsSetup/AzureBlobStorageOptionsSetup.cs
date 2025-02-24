using Lazy.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace Lazy.App.OptionsSetup;

public class AzureBlobStorageOptionsSetup : IConfigureOptions<AzureBlobStorageOptions>
{
    private const string SectionName = "AzureBlobStorage";
    private readonly IConfiguration _configuration;

    public AzureBlobStorageOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(AzureBlobStorageOptions options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}