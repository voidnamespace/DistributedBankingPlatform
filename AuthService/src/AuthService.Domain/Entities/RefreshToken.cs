namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
    public DateTime ExpiresAt
    {
        get => ExpiryDate;
        set => ExpiryDate = value;
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;

    private RefreshToken() { }

    public RefreshToken(string token, Guid userId, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null, empty, or whitespace.", nameof(token));

        Id = Guid.NewGuid();
        Token = token;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        RevokedAt = null;
        IsRevoked = false;
    }

    public bool IsActive()
    {
        return !IsRevoked && ExpiryDate > DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiryDate;
    }

    public void Revoke()
    {
        if (IsRevoked)
            return;

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
