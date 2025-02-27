using Lazy.Application.Abstractions.Email;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Lazy.Application.Users.ForgotPassword;

public class ForgotPasswordCommandHandler(
    ILogger<ForgotPasswordCommandHandler> logger,
    IEmailService emailService,
    UserManager<User> userManager)
    : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Forgot password request");
        
        var amnesiaUser = await userManager.FindByEmailAsync(request.Email);
        if (amnesiaUser is null)
        {
            //do not expose that a user with this email doesn't exist
            return Result.Success();
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(amnesiaUser);
        await emailService.SendForgotPasswordEmailAsync(amnesiaUser, resetToken);
        
        return Result.Success();
    }
}