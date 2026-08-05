namespace AuthService.Application.Abstractions.EmailConfirmation;

public interface IEmailConfirmationTokenStore
{
    Task StoreAsync(
        string tokenHash,
        Guid userId,
        TimeSpan lifetime,
        CancellationToken cancellationToken);

    Task<Guid?> GetUserIdAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string tokenHash,
        CancellationToken cancellationToken);
}
