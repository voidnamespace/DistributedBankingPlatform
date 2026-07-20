namespace AuthService.Application.Abstractions.EmailConfirmation;

public interface IEmailSender
{
    Task SendAsync(
    string recipientEmail,
    CancellationToken cancellationToken);
}
