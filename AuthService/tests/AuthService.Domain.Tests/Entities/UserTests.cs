using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Events;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthService.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateId()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = new User(email, password);

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldSetEmail()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = new User(email, password);

        // Assert
        user.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_ShouldSetPasswordHash()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = new User(email, password);

        // Assert
        user.PasswordHash.Should().Be(password);
    }

    [Fact]
    public void Constructor_ShouldSetRoleToCustomer()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = new User(email, password);

        // Assert
        user.Role.Should().Be(Roles.Customer);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");
        var before = DateTime.UtcNow;

        // Act
        var user = new User(email, password);

        // Assert
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = new User(email, password);

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldAddUserCreatedDomainEvent()
    {
        // Arrange
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = new User(email, password);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserCreatedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<UserCreatedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.Email.Should().Be(email);
    }

    [Fact]
    public void CreateAdmin_ShouldSetRoleToAdmin()
    {
        // Arrange
        var email = new EmailVO("admin@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = User.CreateAdmin(email, password);

        // Assert
        user.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public void CreateAdmin_ShouldSetCreatedAt()
    {
        // Arrange
        var email = new EmailVO("admin@example.com");
        var password = PasswordVO.FromHash("hash-1");
        var before = DateTime.UtcNow;

        // Act
        var user = User.CreateAdmin(email, password);

        // Assert
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void CreateAdmin_ShouldCreateId()
    {
        // Arrange
        var email = new EmailVO("admin@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = User.CreateAdmin(email, password);

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateAdmin_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var email = new EmailVO("admin@example.com");
        var password = PasswordVO.FromHash("hash-1");

        // Act
        var user = User.CreateAdmin(email, password);

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChangeEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = CreateUser();
        var newEmail = new EmailVO("bob@example.com");
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void ChangeEmail_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var user = CreateUser();
        var newEmail = new EmailVO("bob@example.com");
        var before = DateTime.UtcNow;
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeOnOrAfter(before);
        user.UpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void ChangeEmail_ShouldAddEmailChangedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        var oldEmail = user.Email;
        var newEmail = new EmailVO("bob@example.com");
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EmailChangedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<EmailChangedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.OldEmail.Should().Be(oldEmail);
        domainEvent.NewEmail.Should().Be(newEmail);
    }

    [Fact]
    public void ChangeEmail_WithSameEmail_ShouldNotChangeStateOrAddEvent()
    {
        // Arrange
        var user = CreateUser();
        var originalEmail = user.Email;
        var originalUpdatedAt = user.UpdatedAt;
        user.ClearDomainEvents();

        // Act
        user.ChangeEmail(EmailVO.Create(originalEmail.Value));

        // Assert
        user.Email.Should().Be(originalEmail);
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangePassword_ShouldUpdatePasswordHash()
    {
        // Arrange
        var user = CreateUser();
        var newPassword = PasswordVO.FromHash("hash-2");
        user.ClearDomainEvents();

        // Act
        user.ChangePassword(newPassword);

        // Assert
        user.PasswordHash.Should().Be(newPassword);
    }

    [Fact]
    public void ChangePassword_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var user = CreateUser();
        var newPassword = PasswordVO.FromHash("hash-2");
        var before = DateTime.UtcNow;
        user.ClearDomainEvents();

        // Act
        user.ChangePassword(newPassword);

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeOnOrAfter(before);
        user.UpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void ChangePassword_ShouldAddPasswordChangedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        var newPassword = PasswordVO.FromHash("hash-2");
        user.ClearDomainEvents();

        // Act
        user.ChangePassword(newPassword);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PasswordChangedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<PasswordChangedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void ChangePassword_WithSamePassword_ShouldNotChangeStateOrAddEvent()
    {
        // Arrange
        var user = CreateUser();
        var originalPassword = user.PasswordHash;
        var samePassword = PasswordVO.FromHash(originalPassword.Hash);
        var originalUpdatedAt = user.UpdatedAt;
        user.ClearDomainEvents();

        // Act
        user.ChangePassword(samePassword);

        // Assert
        user.PasswordHash.Should().Be(originalPassword);
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangeRole_ShouldUpdateRole()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.ChangeRole(Roles.Admin);

        // Assert
        user.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public void ChangeRole_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var user = CreateUser();
        var before = DateTime.UtcNow;
        user.ClearDomainEvents();

        // Act
        user.ChangeRole(Roles.Admin);

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeOnOrAfter(before);
        user.UpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void ChangeRole_ShouldAddRoleChangedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        var oldRole = user.Role;
        var newRole = Roles.Admin;
        user.ClearDomainEvents();

        // Act
        user.ChangeRole(newRole);

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RoleChangedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<RoleChangedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.OldRole.Should().Be(oldRole);
        domainEvent.NewRole.Should().Be(newRole);
    }

    [Fact]
    public void ChangeRole_WithSameRole_ShouldNotChangeStateOrAddEvent()
    {
        // Arrange
        var user = CreateUser();
        var originalUpdatedAt = user.UpdatedAt;
        user.ClearDomainEvents();

        // Act
        user.ChangeRole(user.Role);

        // Assert
        user.Role.Should().Be(Roles.Customer);
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_FromInactive_ShouldSetUserToActive()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();
        user.Deactivate();
        user.ClearDomainEvents();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_FromInactive_ShouldAddUserActivatedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();
        user.Deactivate();
        user.ClearDomainEvents();

        // Act
        user.Activate();

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserActivatedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<UserActivatedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotChangeStateOrAddEvent()
    {
        // Arrange
        var user = CreateUser();
        var originalUpdatedAt = user.UpdatedAt;
        user.ClearDomainEvents();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().Be(originalUpdatedAt);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_FromActive_ShouldSetUserToInactive()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_FromActive_ShouldAddUserDeactivatedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.Deactivate();

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserDeactivatedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<UserDeactivatedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotChangeStateOrAddEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();
        user.Deactivate();
        var updatedAtAfterFirstDeactivate = user.UpdatedAt;
        user.ClearDomainEvents();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().Be(updatedAtAfterFirstDeactivate);
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Delete_ShouldAddUserDeletedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();

        // Act
        user.Delete();

        // Assert
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserDeletedDomainEvent>();

        var domainEvent = user.DomainEvents.OfType<UserDeletedDomainEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void Touch_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var user = CreateUser();
        var before = DateTime.UtcNow;
        user.ClearDomainEvents();

        // Act
        user.Touch();

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeOnOrAfter(before);
        user.UpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
        user.DomainEvents.Should().BeEmpty();
    }

    private static User CreateUser()
    {
        var email = new EmailVO("alice@example.com");
        var password = PasswordVO.FromHash("hash-1");

        return new User(email, password);
    }
}
