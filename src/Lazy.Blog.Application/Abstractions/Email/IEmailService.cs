using Lazy.Domain.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Lazy.Application.Abstractions.Email;

public interface IEmailService : IEmailSender
{
    Task SendForgotPasswordEmailAsync(User amnesiaUser, string resetToken);
}