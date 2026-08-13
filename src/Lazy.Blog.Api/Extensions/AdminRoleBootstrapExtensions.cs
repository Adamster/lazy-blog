using Lazy.Domain.Entities;
using Lazy.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Lazy.Blog.Api.Extensions;

internal static class AdminRoleBootstrapExtensions
{
    private const string ConfigurationKey = "Authorization:BootstrapAdminUserName";
    private const string CompletionClaimType = "notlazy/bootstrap";
    private const string CompletionClaimValue = "admin-assigned";

    internal static async Task BootstrapAdminRoleAsync(this WebApplication app)
    {
        string? userName = app.Configuration[ConfigurationKey];
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException($"Configuration value '{ConfigurationKey}' is required.");
        }

        await using var scope = app.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminRoleBootstrap");

        Role? role = await roleManager.FindByNameAsync(Role.AdminRoleName);
        if (role is null)
        {
            EnsureSucceeded(await roleManager.CreateAsync(new Role { Name = Role.AdminRoleName }), "create Admin role");
            role = await roleManager.FindByNameAsync(Role.AdminRoleName)
                ?? throw new InvalidOperationException("Admin role was created but could not be loaded.");
        }

        if ((await roleManager.GetClaimsAsync(role)).Any(claim =>
                claim.Type == CompletionClaimType && claim.Value == CompletionClaimValue)) return;

        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            logger.LogWarning("Admin bootstrap user {UserName} was not found.", userName);
            return;
        }

        if (!await userManager.IsInRoleAsync(user, Role.AdminRoleName))
            EnsureSucceeded(await userManager.AddToRoleAsync(user, Role.AdminRoleName), $"assign Admin role to {userName}");
        EnsureSucceeded(await roleManager.AddClaimAsync(role, new Claim(CompletionClaimType, CompletionClaimValue)),
            "mark Admin bootstrap complete");
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded) return;
        throw new InvalidOperationException($"Could not {operation}: {string.Join(", ", result.Errors.Select(error => error.Description))}");
    }
}
