using AuthService.Application.Common.Events;
using AuthService.Domain.Entities;
using AuthService.Domain.Events;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Persistence.Outbox;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using UnitOfWorkType = AuthService.Infrastructure.Persistence.UnitOfWork.UnitOfWork;

namespace AuthService.Infrastructure.Tests.Persistence.UnitOfWork;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_WithDomainEvents_ShouldPersistStateBeforePublishingAndThenPersistOutboxChanges()
    {
        // Arrange
        await using var context = CreateContext();
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<UnitOfWorkType>>();
        var unitOfWork = new UnitOfWorkType(
            context,
            mediatorMock.Object,
            loggerMock.Object);

        var user = new User(
            new EmailVO("alice@example.com"),
            new PasswordVO("SecurePassword123"));

        context.Users.Add(user);

        mediatorMock
            .Setup(mediator => mediator.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns<INotification, CancellationToken>((notification, ct) =>
            {
                context.SaveChangesCalls.Should().Be(1);
                context.Users.AsNoTracking().Should().ContainSingle(savedUser => savedUser.Id == user.Id);

                var domainNotification = notification as DomainEventNotification<UserCreatedDomainEvent>;
                domainNotification.Should().NotBeNull();

                context.OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = "user.created",
                    Payload = $"{{\"userId\":\"{user.Id}\"}}",
                    CreatedAt = DateTime.UtcNow,
                    AttemptCount = 0
                });

                return Task.CompletedTask;
            });

        // Act
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        context.SaveChangesCalls.Should().Be(2);
        mediatorMock.Verify(
            mediator => mediator.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        context.OutboxMessages.AsNoTracking().Should().ContainSingle(message => message.Type == "user.created");
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WithoutDomainEvents_ShouldPersistStateWithoutPublishing()
    {
        // Arrange
        await using var context = CreateContext();
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<UnitOfWorkType>>();
        var unitOfWork = new UnitOfWorkType(
            context,
            mediatorMock.Object,
            loggerMock.Object);

        var user = new User(
            new EmailVO("alice@example.com"),
            new PasswordVO("SecurePassword123"));
        user.ClearDomainEvents();

        context.Users.Add(user);

        // Act
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        // Assert
        context.SaveChangesCalls.Should().Be(1);
        mediatorMock.Verify(
            mediator => mediator.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.Users.AsNoTracking().Should().ContainSingle(savedUser => savedUser.Id == user.Id);
    }

    private static RecordingAuthDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new RecordingAuthDbContext(options);
    }

    private sealed class RecordingAuthDbContext : AuthDbContext
    {
        public RecordingAuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public int SaveChangesCalls { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
