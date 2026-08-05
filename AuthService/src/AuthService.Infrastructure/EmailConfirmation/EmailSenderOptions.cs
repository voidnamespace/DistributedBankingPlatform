namespace AuthService.Infrastructure.EmailConfirmation;

public sealed class EmailSenderOptions
{
    public const string SectionName = "EmailSender";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
}
