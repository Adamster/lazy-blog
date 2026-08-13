using Lazy.Domain.Entities;
using Lazy.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Lazy.Blog.Api.Extensions;

internal static class AdminRoleBootstrapExtensions
{
    private const string ConfigurationKey = "Authorization:BootstrapAdminEmails";
    private const string CompletionClaimType = "notlazy/bootstrap";
    private const string CompletionClaimValue = "admin-assigned";

    internal static async Task BootstrapAdminRoleAsync(this WebApplication app)
    {
        string[] emails = app.Configuration.GetSection(ConfigurationKey).Get<string[]>() ?? [];
        if (emails.Length == 0)
        {
            throw new InvalidOperationException(
                $"Configuration value '{ConfigurationKey}' must list at least one admin email.");
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

        bool anyAssigned = false;
        foreach (string email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.LogWarning("Admin bootstrap email {Email} was not found.", email);
                continue;
            }

            anyAssigned = true;
            if (!await userManager.IsInRoleAsync(user, Role.AdminRoleName))
                EnsureSucceeded(
                    await userManager.AddToRoleAsync(user, Role.AdminRoleName),
                    $"assign Admin role to {email}");
        }

        // Keep retrying on every startup until at least one configured admin account exists.
        // Once granted, the completion claim prevents an explicit role removal from being
        // silently reapplied by a later restart.
        if (!anyAssigned) return;

        EnsureSucceeded(await roleManager.AddClaimAsync(role, new Claim(CompletionClaimType, CompletionClaimValue)),
            "mark Admin bootstrap complete");
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded) return;
        string errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Could not {operation}: {errors}");
    }
}
