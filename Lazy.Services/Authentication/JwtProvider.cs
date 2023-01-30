using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lazy.DataContracts.Author;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lazy.Services.Authentication;

public sealed  class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _options;

    public JwtProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(AuthorItemDto author)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, author.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, author.AuthorEmail)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256); 
        
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            null,
            DateTime.UtcNow.AddHours(1),
            signingCredentials 
        );

        var tokenValue = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return tokenValue;
    }
}