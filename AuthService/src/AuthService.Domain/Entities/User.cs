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
            new UserCreatedDomainEvent(Id, Email));
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
        if (Email == newEmail)
            return;

        var oldEmail = Email;

        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new EmailChangedDomainEvent(Id, oldEmail, newEmail));
    }

    public void ChangePassword(PasswordVO newPassword)
    {
        if (PasswordHash == newPassword)
            return;

        PasswordHash = newPassword;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(
            new PasswordChangedDomainEvent(Id));
    }

    public void ChangeRole(Roles newRole)
    {
        if (Role == newRole)
            return;
        
        var oldRole = Role;

        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(
            new RoleChangedDomainEvent(Id, oldRole, newRole));
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

    public void Delete()
    {
        AddDomainEvent(
            new UserDeletedDomainEvent(Id));
    }
}
