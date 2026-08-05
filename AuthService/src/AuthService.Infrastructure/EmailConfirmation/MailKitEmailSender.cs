using AuthService.Application.Abstractions.EmailConfirmation;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthService.Infrastructure.EmailConfirmation;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly EmailSenderOptions _options;

    public MailKitEmailSender(IOptions<EmailSenderOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string message,
        CancellationToken cancellationToken)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(
            _options.SenderName,
            _options.SenderEmail));
        email.To.Add(MailboxAddress.Parse(recipientEmail));
        email.Subject = subject;
        email.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.None,
            cancellationToken);

        await smtpClient.SendAsync(email, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
