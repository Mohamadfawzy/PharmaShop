using System.Net;
using System.Net.Mail;
using Contracts.Notifications;

namespace WebAPI.Notifications;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    public SmtpEmailSender(IConfiguration configuration) => _configuration = configuration;

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var host = _configuration["Smtp:Host"];
        var from = _configuration["Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("SMTP is not configured. Set Smtp:Host and Smtp:From.");
        var port = int.TryParse(_configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
        using var message = new MailMessage(from, toEmail, subject, htmlBody) { IsBodyHtml = true };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) && ssl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(username)) client.Credentials = new NetworkCredential(username, password);
        await client.SendMailAsync(message, ct);
    }
}
