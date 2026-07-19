namespace AuthService.Application.Abstractions.Authentication;

public interface ITokenBlacklistStore
{
    Task BlacklistAsync(
        string tokenId,
        TimeSpan expiresIn,
        CancellationToken cancellationToken);

    Task<bool> IsBlacklistedAsync(
        string tokenId,
        CancellationToken cancellationToken);
}
