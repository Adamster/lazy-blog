using Lazy.Application.Abstractions;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Abstractions.User;
using Lazy.Application.Users.GetUserById;
using Lazy.Application.Users.Login;
using Lazy.Domain.Shared;

namespace Lazy.Application.Identity.ExternalAuth;

public class ExternalLoginCommandHandler(IJwtProvider jwtProvider, IUserService userService)
    : ICommandHandler<ExternalLoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(ExternalLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.FindByExternalLoginAsync(request.Provider, request.ProviderUserId);

        if (user is null)
        {
            user = await userService.CreateExternalUserAsync(request.Email, request.Provider, request.ProviderUserId);
        }

        var tokenResponse = await jwtProvider.GenerateAsync(user, cancellationToken);

        return new LoginResponse(
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            new UserResponse(user));
    }
}