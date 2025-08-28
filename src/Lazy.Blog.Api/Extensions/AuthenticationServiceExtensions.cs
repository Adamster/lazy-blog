using Lazy.Application.Abstractions.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Blog.Api.Extensions;

public static class AuthenticationServiceExtensions
{
    public static AuthenticationBuilder AddExternalAuthentication(this AuthenticationBuilder authenticationBuilder, IConfiguration config)
    {


        authenticationBuilder
            .AddGoogle(googleOptions =>
        {
            config.GetSection("Authentication:Google").Bind(googleOptions);

            googleOptions.SaveTokens = true;
            googleOptions.SignInScheme = IdentityConstants.ExternalScheme;

            googleOptions.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
            googleOptions.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");




        });
        //.AddFacebook(options =>
        //{
        //    config.GetSection("Authentication:Facebook").Bind(options);
        //})
        //.AddGitHub(options =>
        //{
        //    config.GetSection("Authentication:GitHub").Bind(options);
        //})
        //.AddMicrosoftAccount(options =>
        //{
        //    config.GetSection("Authentication:Microsoft").Bind(options);
        //});


        return authenticationBuilder;
    }
}