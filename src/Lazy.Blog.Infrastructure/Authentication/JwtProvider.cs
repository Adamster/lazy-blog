using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Lazy.Application.Abstractions;
using Lazy.Application.Users.RefreshToken;
using Lazy.Domain.Entities;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Repositories.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lazy.Infrastructure.Authentication;

public sealed class JwtProvider(
    IOptions<JwtOptions> options,
    IUserTokenRepository userTokenRepository,
    IOptions<JwtBearerOptions> jwtBearerOptions)
    : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;
    private readonly JwtBearerOptions _jwtBearerOptions = jwtBearerOptions.Value;
    private const int TokenLifeTimeInMinutes = 30;


    public async Task<TokenResponse> GenerateAsync(User user, CancellationToken cancellationToken)
    {
        var userRole = user.UserRoles.FirstOrDefault()?.Role;
        
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Email, user.Email!),
            new (JwtRegisteredClaimNames.Name, $"{user.FirstName.Value} {user.LastName.Value}")
        };

        if (userRole?.Name != null)
        {
            var roleClaim =  new Claim(ClaimTypes.Role, userRole.Name);
            claims.Add(roleClaim);
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            null,
            DateTime.UtcNow.AddMinutes(TokenLifeTimeInMinutes),
            signingCredentials);

        string tokenValue = new JwtSecurityTokenHandler()
            .WriteToken(token);

        var userToken = new UserToken(token.Id, user);

        await userTokenRepository.AddAsync(userToken, cancellationToken);

        return new TokenResponse(tokenValue, userToken.Value!);
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token,
                _jwtBearerOptions.TokenValidationParameters,
                out var validatedToken);

            return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    public bool IsTokenExpired(ClaimsPrincipal validatedToken)
    {
        var expiryTokenValue =
            validatedToken
                .Claims
                .Single(x => x.Type == JwtRegisteredClaimNames.Exp)
                .Value;

        long expiryDateUnix = long.Parse(expiryTokenValue);

        DateTime expiryDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expiryDateUnix)
            .UtcDateTime;

        return expiryDateTimeUtc < DateTime.UtcNow;
    }

    public string GetAccessTokenId(ClaimsPrincipal validatedToken)
    {
        var tokenIdValue =
            validatedToken
                .Claims
                .Single(x => x.Type == JwtRegisteredClaimNames.Jti)
                .Value;

        return tokenIdValue;
    }

    public Guid GetUserIdFromToken(ClaimsPrincipal validatedToken)
    {
        var userIdValue = validatedToken
            .Claims
            .Single(x => x.Type == ClaimTypes.NameIdentifier)
            .Value;
        var userId = Guid.Parse(userIdValue);

        return userId;
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return validatedToken is JwtSecurityToken jwtSecurityToken &&
               jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                   StringComparison.InvariantCultureIgnoreCase);
    }
}
