using System.Security.Claims;
using Lazy.Application.Abstractions.Authorization;
using Microsoft.AspNetCore.Http;

namespace Lazy.Infrastructure.Authorization;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsCurrentUser(Guid userId)
    {
        if (_httpContextAccessor == null)
        {
            throw new NullReferenceException($"{nameof(_httpContextAccessor)} is null");
        }

        if (_httpContextAccessor.HttpContext == null)
        {
            return false;
        }

        Claim currentUserClaim = _httpContextAccessor
            .HttpContext
            .User
            .Claims
            .Single(c => c.Type == ClaimTypes.NameIdentifier);

        return currentUserClaim.Value == userId.ToString();

    }
}