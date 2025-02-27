using Lazy.Infrastructure.Services.Impl;
using Microsoft.Extensions.Options;

namespace Lazy.Blog.Api.OptionsSetup;

public class SendGridOptionsSetup(IConfiguration configuration) : IConfigureOptions<SendGridOptions>
{
    private const string SectionName = "SendGrid";

    public void Configure(SendGridOptions options)
    {
        configuration.GetSection(SectionName).Bind(options);
    }
}