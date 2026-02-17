using AuthService.Domain.Enums;
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
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

}
