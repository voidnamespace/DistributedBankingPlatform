namespace AuthService.Domain.ValueObjects;

public class EmailVO : IEquatable<EmailVO>
{
    public string _email { get; private set; } = string.Empty;

    private EmailVO() { }

    public string Value => _email;

    public EmailVO(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null, empty, or whitespace.", nameof(email));

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                throw new ArgumentException("Invalid email format.", nameof(email));
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }

        _email = email;
    }

    public static EmailVO Create(string email)
    {
        return new EmailVO(email);
    }

    public override string ToString()
    {
        return Value;
    }

    public bool Equals(EmailVO? other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is EmailVO other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    public static bool operator ==(EmailVO? left, EmailVO? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EmailVO? left, EmailVO? right)
    {
        return !Equals(left, right);
    }
}
