using Contracts.Notifications;

namespace WebAPI.Notifications;


public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        _logger.LogInformation("EMAIL to={To}, subject={Subject}, body={Body}", toEmail, subject, htmlBody);
        return Task.CompletedTask;
    }
}