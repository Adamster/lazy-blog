using Lazy.Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Lazy.Application.Abstractions.Email;

public interface IEmailService : IEmailSender
{
    Task SendForgotPasswordEmailAsync(Domain.Entities.User amnesiaUser, string resetToken);
    Task SendWelcomeEmail(string userEmail, string userName);
}