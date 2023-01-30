using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lazy.DataContracts.Author;
using Microsoft.IdentityModel.Tokens;

namespace Lazy.Services.Providers;

public sealed  class JwtProvider : IJwtProvider
{
    public string Generate(AuthorItemDto author)
    {
        var claims = new Claim[] { };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("security-key")),
            SecurityAlgorithms.HmacSha256); 
        
        var token = new JwtSecurityToken(
            "issuer",
            "audience",
            claims,
            null,
            DateTime.UtcNow.AddHours(1),
            signingCredentials 
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenValue;
    }
}