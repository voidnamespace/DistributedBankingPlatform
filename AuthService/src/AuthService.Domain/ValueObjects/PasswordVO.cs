
namespace AuthService.Domain.ValueObjects;

using BCrypt.Net;

public class PasswordVO : IEquatable<PasswordVO>
{
    private string _hash = string.Empty;
    public string Hash => _hash;

    private PasswordVO() { }

    public static PasswordVO FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be null, empty, or whitespace.", nameof(hash));

        return new PasswordVO { _hash = hash };
    }

    public PasswordVO(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
            throw new ArgumentNullException(nameof(plainPassword));
        if (plainPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");
        _hash = BCrypt.HashPassword(plainPassword);
    }

    public static PasswordVO Create(string plainPassword)
    {
        return new PasswordVO(plainPassword);
    }

    public override string ToString()
    {
        return "PasswordVO(****)";
    }

    public bool Equals(PasswordVO? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(Hash, other.Hash, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is PasswordVO other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Hash);
    }

    public static bool operator ==(PasswordVO? left, PasswordVO? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PasswordVO? left, PasswordVO? right)
    {
        return !Equals(left, right);
    }
}
