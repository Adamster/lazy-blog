using System.Security.Claims;
using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.User;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ApplicationException = System.ApplicationException;
using Email = Lazy.Domain.ValueObjects.User.Email;
using FirstName = Lazy.Domain.ValueObjects.User.FirstName;
using LastName = Lazy.Domain.ValueObjects.User.LastName;
using User = Lazy.Domain.Entities.User;
using UserName = Lazy.Domain.ValueObjects.User.UserName;

namespace Lazy.Infrastructure.Services.Impl;

public class UserService(UserManager<User> userManager, 
    SignInManager<User> signInManager,
    IJwtProvider jwtProvider,
    ILogger<UserService> logger) : IUserService
{
    public async Task<User?> FindByExternalLoginAsync(string provider, string providerUserId)
    {
        return await userManager.FindByLoginAsync(provider, providerUserId);
    }

    public async Task<User> CreateExternalUserAsync(string email, string provider, string providerUserId)
    {
        var emailResult = Email.Create(email);

        var user = User.Create(emailResult, provider, providerUserId);

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
            throw new Exception("Failed to create user: " +
                                string.Join(", ", result.Errors.Select(e => e.Description)));

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

        User? existingUser = null;

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
                var addLoginRes = await userManager.AddLoginAsync(existingUser, info);

            }
        }


        return existingUser!;


        //var firstNameValue = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? $"External {provider}";
        //var lastNameValue = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? "User";
        //var userNameValue = claimsPrincipal.FindFirstValue(ClaimTypes.Email)!.Split('@')[0];

        //var emailResult = Email.Create(email);
        //var lastNameResult = LastName.Create(lastNameValue);
        //var firstNameResult = FirstName.Create(firstNameValue);
        //var userNameResult = UserName.Create(userNameValue);
    }

    public async Task<LoginResponse> CreateOrLoginUser(ExternalLoginInfo info)
    {
        User? user = null;
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

            if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));

            user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var userId = Guid.NewGuid();
                var emailResult = Email.Create(email);
                var firstName = FirstName.Create(info.Principal.FindFirstValue(ClaimTypes.GivenName)!);
                var lastName = LastName.Create(info.Principal.FindFirstValue(ClaimTypes.Surname)!);
                var userName = UserName.Create(email.Split('@')[0]);

                user = User.Create(userId, emailResult.Value, firstName.Value,
                    lastName.Value,
                    userName.Value,
                    null);


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

        return new LoginResponse(tokenResponse.AccessToken,  tokenResponse.RefreshToken, new UserResponse(user!));
    }
}