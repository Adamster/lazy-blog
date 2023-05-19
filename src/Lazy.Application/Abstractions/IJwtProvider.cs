using System.Security.Claims;
using Lazy.Domain.Entities;

namespace Lazy.Application.Abstractions;

public interface IJwtProvider
{
    string Generate(User user);

    ClaimsPrincipal? GetPrincipalFromToken(string token);
}