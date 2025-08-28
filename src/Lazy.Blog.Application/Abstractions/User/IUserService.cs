using Lazy.Application.Users.Login;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Lazy.Application.Abstractions.User;

public interface IUserService
{
    Task<Domain.Entities.User?> FindByExternalLoginAsync(string provider, string providerUserId);
    Task<Domain.Entities.User> CreateExternalUserAsync(string email, string provider, string providerUserId);
    Task<Domain.Entities.User> CreateExternalUserAsync(ClaimsPrincipal claimsPrincipal);
    Task<LoginResponse> CreateOrLoginUser(ExternalLoginInfo infoPrincipal);
}