using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using AuthService.Domain.ValueObjects;
namespace AuthService.Domain.Entities;

public class User : Entity
{

    public Guid Id { get; private set; }
    public EmailVO Email { get; private set; } = null!;

    public PasswordVO PasswordHash { get; private set; } = null!;

    public Roles Role { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; private set; }

    public bool IsActive { get; private set; } = true;

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    private User() { }
    public User(EmailVO email, PasswordVO password)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = password;
        Role = Roles.Customer;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new UserCreatedDomainEvent(Id, Email.Value));
    }

    public static User CreateAdmin(EmailVO email, PasswordVO password)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = password,
            Role = Roles.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void ChangeEmail(EmailVO newEmail)
    {
        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(PasswordVO newPassword)
    {
        PasswordHash = newPassword;
        UpdatedAt = DateTime.UtcNow;
    }
    public void ChangeRole(Roles newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new UserDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new UserActivatedDomainEvent(Id));
    }

}
