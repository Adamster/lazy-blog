using Lazy.Application.Abstractions.Email;
using Lazy.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Lazy.Infrastructure.Services.Impl;

public class SendGridEmailSender(IOptions<SendGridOptions> options, ILogger<SendGridEmailSender> logger)
    : IEmailService
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var apikey = options.Value.ApiKey;
        if (string.IsNullOrEmpty(apikey))
        {
            throw new ArgumentNullException(nameof(apikey));
        }

        var client = new SendGridClient(apikey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress("noreply@notlazy.org", "Lazy"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message,
        };
        
        msg.AddTo(new EmailAddress(email));
        
        var response = await client.SendEmailAsync(msg);
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Email sent successfully");
        }
        else
        {
            logger.LogError("Email sent failed");
        }
    }

    public async Task SendForgotPasswordEmailAsync(User amnesiaUser, string token)
    {
        string resetLink = $"https://notlazy.org/auth/reset-password?token={token}&id={amnesiaUser.Id}";
        string emailContent = ForgotEmailContent.EmailBody
            .Replace("{{reset_link}}", resetLink)
            .Replace("{{user}}", amnesiaUser.UserName)
            .Replace("{{plain_url}}", resetLink);

        await SendEmailAsync(amnesiaUser.Email!, "Password reset", emailContent);
    }

    public async Task SendWelcomeEmail(string userEmail, string userName)
    {
        string emailContent = WelcomeEmailContent.EmailBody
            .Replace("{{user_name}}", userName);
        
        await SendEmailAsync(userEmail, "Welcome to Not Lazy blog", emailContent);
    }

    private class ForgotEmailContent
    {
        public static string EmailBody = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Reset Your Password – No Rush, Though!</title>
    <style>
        body {
            font-family: 'Arial', sans-serif;
            background-color: #f0f0f0;
            color: #333;
            margin: 0;
            padding: 0;
            text-align: center;
        }
        .container {
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        .content {
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }
        h2 {
            color: #2c3e50;
        }
        p {
            font-size: 16px;
            line-height: 1.5;
        }
        .button {
            display: inline-block;
            background-color: #3498db;
            color: #ffffff;
            padding: 12px 20px;
            text-decoration: none;
            border-radius: 5px;
            font-size: 16px;
            font-weight: bold;
            margin: 20px 0;
        }
        .footer {
            margin-top: 20px;
            font-size: 14px;
            color: #777;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <h2>😴 Reset Your Password – No Rush, Though!</h2>
            <p>Hey {{user}},<br><br>
            We get it—remembering passwords is <em>hard</em>. And honestly, who has the energy for that? 
            No worries, though! You can reset your password with just one click (or whenever you feel like it).</p>

            <a href='{{reset_link}}' class='button'>🔑 Reset Password</a>
            <p> If link is not working please try this url: {{plain_url}}</p>
            <p>No need to hurry! This link is good for <strong>24 hours</strong> (plenty of time for a nap first).<br>
            If you didn’t request this, just ignore it—doing nothing is totally on brand.</p>

            <p class='footer'>
                Stay lazy, <br>
                <strong>Not Lazy Blog</strong>
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private class WelcomeEmailContent
    {
       public static string EmailBody = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Welcome to Not Lazy!</title>
    <style>
        body {
            font-family: 'Arial', sans-serif;
            background-color: #f0f0f0;
            color: #333;
            margin: 0;
            padding: 0;
            text-align: center;
        }
        .container {
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        .content {
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
        }
        h2 {
            color: #2c3e50;
        }
        p {
            font-size: 16px;
            line-height: 1.5;
        }
        .button {
            display: inline-block;
            background-color: #3498db;
            color: #ffffff;
            padding: 12px 20px;
            text-decoration: none;
            border-radius: 5px;
            font-size: 16px;
            font-weight: bold;
            margin: 20px 0;
        }
        .footer {
            margin-top: 20px;
            font-size: 14px;
            color: #777;
        }
        .ascii-sloth {
            font-family: monospace;
            white-space: pre-wrap;
            font-size: 12px;
            color: #555;
            margin-top: 15px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <h2>🎉 Welcome to Not Lazy!</h2>
            <p>Hey there,{{user_name}}<br><br>
            We're thrilled you’ve decided to join the laziest corner of the internet. Take your time, get comfy, and explore at your own pace—there's no rush here.</p>

            <a href='https://t.me/notlazyblog' class='button'>🚀 Get Started</a>

            <p>Whether you’re here to read, relax, or procrastinate (no judgment), we're happy to have you. If you need anything, just reach out—though we totally get if you don't feel like it. 😎</p>

            <div class='ascii-sloth'>
                <pre>
                  __
                 (oO) 
                /-||-\
                 ||||  
                _||||_
                </pre>
            </div>

            <p class='footer'>
                Stay lazy, <br>
                <strong>Not Lazy Team</strong>
            </p>
        </div>
    </div>
</body>
</html>";

    }
}