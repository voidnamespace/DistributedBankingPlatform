namespace AuthService.Application.Abstractions.EmailConfirmation;

public interface IEmailSender
{
    Task SendAsync(
        string recipientEmail,
        string subject,
        string message,
        CancellationToken cancellationToken);
}
