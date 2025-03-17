using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories.Identity;
using Lazy.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lazy.Application.Users.ChangePassword;

public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly ILogger<ChangePasswordCommandHandler> _logger;
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUserTokenRepository _userTokenRepository;


    public ChangePasswordCommandHandler(ILogger<ChangePasswordCommandHandler> logger, UserManager<User> userManager,
        ICurrentUserContext currentUserContext, IUserTokenRepository userTokenRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _currentUserContext = currentUserContext;
        _userTokenRepository = userTokenRepository;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Change Password Command received");

        var userId = _currentUserContext.GetCurrentUserId();

        var userToChangePassword = await _userManager.FindByIdAsync(userId.ToString());

        if (userToChangePassword is null)
        {
            _logger.LogCritical("User does not exist");
            return Result.Failure<ChangePasswordCommand>(DomainErrors.User.NotFound(userId));
        }

        var changeResult =
            await _userManager.ChangePasswordAsync(userToChangePassword, request.OldPassword, request.NewPassword);

        if (!changeResult.Succeeded)
        {
            return Result.Failure<ChangePasswordCommand>(
                DomainErrors.User.ChangePasswordFailed(changeResult.Errors.First()));
        }

        _logger.LogInformation("Successfully changed Password");
        
        var refreshTokens = await _userTokenRepository.GetAllByUserIdAsync(userId, cancellationToken);

        _logger.LogInformation("Invalidating all existing refresh tokens");
        foreach (var token in refreshTokens)
        {
            token.Invalidate();
            _userTokenRepository.Update(token);
        }
        
        _logger.LogInformation("Successfully changed all existing refresh tokens");
        return Result.Success();
    }
}