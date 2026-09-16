namespace REAK.Api.Services.Notifications;

/// <summary>Placeholder IEmailSender: logs instead of delivering. No SMTP/transactional-email
/// provider is configured for this project yet, so claiming to have "sent" an email here would be
/// exactly the kind of fake data flow spec §35 prohibits. Every caller of IEmailSender should be
/// written as if delivery were real (real templates, real recipients) so that swapping this for an
/// actual provider (SendGrid, SES, SMTP) in Program.cs's DI registration is the only production
/// change needed — flagged in the production-readiness checklist (spec §34).</summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        logger.LogWarning(
            "EMAIL NOT SENT (no email provider configured) — To: {To}, Subject: {Subject}\n{Body}",
            toEmail, subject, body);
        return Task.CompletedTask;
    }
}
