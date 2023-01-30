using Lazy.Services.Authentication;
using Microsoft.Extensions.Options;

namespace Lazy.Presentation.OptionsSetup
{
    public class JwtOptionSetup : IConfigureOptions<JwtOptions>
    {
        private const string SectionName = "Jwt";
        private readonly IConfiguration _configuration;

        public JwtOptionSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(JwtOptions options)
        {
            _configuration.GetSection(SectionName).Bind(options);
        }
    }
}
