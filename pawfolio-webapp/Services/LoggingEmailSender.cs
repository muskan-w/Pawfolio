using Microsoft.AspNetCore.Identity.UI.Services;

namespace pawfolio_webapp.Services;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation(
            "Development email to {Email}, subject {Subject}: {Message}",
            email,
            subject,
            htmlMessage);

        return Task.CompletedTask;
    }
}
