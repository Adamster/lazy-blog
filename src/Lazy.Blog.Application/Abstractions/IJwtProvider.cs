using System.Security.Claims;
using Lazy.Application.Users.RefreshToken;
using Lazy.Domain.Entities;

namespace Lazy.Application.Abstractions;

public interface IJwtProvider
{
    Task<TokenResponse> GenerateAsync(Domain.Entities.User user, CancellationToken cancellationToken);

    ClaimsPrincipal? GetPrincipalFromToken(string token);

    bool IsTokenExpired(ClaimsPrincipal validatedToken);

    string GetAccessTokenId(ClaimsPrincipal validatedToken);

    Guid GetUserIdFromToken(ClaimsPrincipal validatedToken);
}