using System.Security.Claims;
using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.User;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ApplicationException = System.ApplicationException;
using DisplayName = Lazy.Domain.ValueObjects.User.DisplayName;
using Email = Lazy.Domain.ValueObjects.User.Email;
using User = Lazy.Domain.Entities.User;
using UserName = Lazy.Domain.ValueObjects.User.UserName;

namespace Lazy.Infrastructure.Services.Impl;

public class UserService(UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtProvider jwtProvider,
    ILogger<UserService> logger) : IUserService
{
    private const string FallbackDisplayName = "user";
    private const string FallbackUserName = "user";

    public async Task<User?> FindByExternalLoginAsync(string provider, string providerUserId)
    {
        return await userManager.FindByLoginAsync(provider, providerUserId);
    }

    public async Task<User> CreateExternalUserAsync(string email, string provider, string providerUserId)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            throw new ApplicationException(emailResult.Error.Message);
        }

        var displayName = BuildDisplayName(EmailLocalPart(email));
        var userName = await GenerateUniqueUserNameAsync(email);

        var user = User.Create(Guid.NewGuid(), emailResult.Value, displayName, userName, null);

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw new ApplicationException("Failed to create user: " +
                                          string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var loginInfo = new UserLoginInfo(provider, providerUserId, provider);
        await userManager.AddLoginAsync(user, loginInfo);

        return user;
    }

    public async Task<User> CreateExternalUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();

        if (info == null) throw new BadHttpRequestException("Failed external login");

        var extSignIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            false);

        User? existingUser;

        if (extSignIn.Succeeded)
        {
            existingUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }
        else
        {
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
            var provider = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(provider))
                throw new ArgumentException("Invalid claims for creating external user.");

            existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                await userManager.AddLoginAsync(existingUser, info);
            }
        }

        return existingUser!;
    }

    public async Task<LoginResponse> CreateOrLoginUser(ExternalLoginInfo info)
    {
        User? user;
        var signIn = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        if (signIn.Succeeded)
        {
            logger.LogInformation("External login succeeded for user {Provider} with key {Key}",
                info.LoginProvider, info.ProviderKey);
            user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }
        else
        {
            logger.LogInformation("External login failed for user {Provider} with key {Key}",
                info.LoginProvider, info.ProviderKey);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        ?? info.Principal.FindFirstValue("email");

            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(info));

            user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var userId = Guid.NewGuid();
                var emailResult = Email.Create(email);
                if (emailResult.IsFailure)
                {
                    throw new ApplicationException(emailResult.Error.Message);
                }

                var displayName = ResolveDisplayName(info.Principal, email);
                var userName = await GenerateUniqueUserNameAsync(email);

                user = User.Create(userId, emailResult.Value, displayName, userName, null);

                var created = await userManager.CreateAsync(user);
                if (!created.Succeeded)
                {
                    throw new ApplicationException("Failed to create user");
                }

                var linked = await userManager.AddLoginAsync(user, info);
                logger.LogInformation("User {UserId} created with email {Email}", user.Id, email);
                if (!linked.Succeeded && linked.Errors.All(e => e.Code != "LoginAlreadyAssociated"))
                {
                    var errors = string.Join(", ", linked.Errors.Select(e => e.Description));
                    throw new ApplicationException(errors);
                }
            }
            else
            {
                var linked = await userManager.AddLoginAsync(user, info);
                if (linked.Succeeded)
                {
                    logger.LogInformation("User {UserId} linked with email {Email}", user.Id, email);
                }
            }
        }

        var tokenResponse = await jwtProvider.GenerateAsync(user!, CancellationToken.None);

        return new LoginResponse(tokenResponse.AccessToken, tokenResponse.RefreshToken, new UserResponse(user!));
    }

    private static DisplayName ResolveDisplayName(ClaimsPrincipal principal, string email)
    {
        var name = principal.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            return BuildDisplayName(name);
        }

        var given = principal.FindFirstValue(ClaimTypes.GivenName);
        var family = principal.FindFirstValue(ClaimTypes.Surname);
        var joined = string.Join(
            ' ',
            new[] { given, family }.Where(part => !string.IsNullOrWhiteSpace(part)));

        if (!string.IsNullOrWhiteSpace(joined))
        {
            return BuildDisplayName(joined);
        }

        return BuildDisplayName(EmailLocalPart(email));
    }

    private static DisplayName BuildDisplayName(string candidate)
    {
        var clamped = Clamp(candidate, DisplayName.MaxLength);
        var result = DisplayName.Create(clamped);

        return result.IsSuccess ? result.Value : DisplayName.Create(FallbackDisplayName).Value;
    }

    private async Task<UserName> GenerateUniqueUserNameAsync(string email)
    {
        var basePart = EmailLocalPart(email);
        if (string.IsNullOrWhiteSpace(basePart))
        {
            basePart = FallbackUserName;
        }

        basePart = Clamp(basePart, UserName.MaxLength);

        var candidate = basePart;
        var suffix = 1;
        while (await userManager.FindByNameAsync(candidate) is not null)
        {
            candidate = $"{basePart}{suffix++}";
        }

        var result = UserName.Create(candidate);

        return result.IsSuccess
            ? result.Value
            : UserName.Create($"{FallbackUserName}{Guid.NewGuid():N}").Value;
    }

    private static string EmailLocalPart(string email)
    {
        var trimmed = email?.Trim() ?? string.Empty;
        var atIndex = trimmed.IndexOf('@');

        return atIndex > 0 ? trimmed[..atIndex] : trimmed;
    }

    private static string Clamp(string value, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
