using System.Diagnostics;
using System.Text;
using Lazy.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lazy.Presentation.OptionsSetup
{
    public class JwtBearerOptionsSetup : IConfigureOptions<JwtBearerOptions>
    {
        private readonly ILogger<JwtBearerOptionsSetup> _logger;
        private readonly JwtOptions _jwtOptions;

        public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions, ILogger<JwtBearerOptionsSetup> logger)
        {
            _logger = logger;
            _logger.LogInformation("START CTOR JwtBearerOptionsSetup");
            _jwtOptions = jwtOptions.Value;
            _logger.LogInformation("END CTOR JwtBearerOptionsSetup");
        }


        public void Configure(JwtBearerOptions options)
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.SecretKey))
            };

            _logger.LogInformation("Configure Configure JwtBearerOptionsSetup");
        }
    }
}
