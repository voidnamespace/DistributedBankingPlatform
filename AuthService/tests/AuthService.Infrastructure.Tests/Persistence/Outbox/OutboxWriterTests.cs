using System.Text.Json;
using AuthService.Application.IntegrationEvents;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Persistence.Outbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthService.Infrastructure.Tests.Persistence.Outbox;

public class OutboxWriterTests
{
    [Fact]
    public async Task EnqueueAsync_WithIntegrationEvent_ShouldStoreCorrectOutboxMessage()
    {
        // Arrange
        await using var context = CreateContext();
        var loggerMock = new Mock<ILogger<OutboxWriter>>();
        var writer = new OutboxWriter(context, loggerMock.Object);
        var integrationEvent = new UserCreatedIntegrationEvent(
            Guid.NewGuid(),
            "alice@example.com");
        var cancellationToken = CancellationToken.None;
        var startedAt = DateTime.UtcNow;

        // Act
        await writer.EnqueueAsync(integrationEvent, cancellationToken);

        // Assert
        var trackedMessages = context.ChangeTracker
            .Entries<OutboxMessage>()
            .Select(entry => entry.Entity)
            .ToList();

        trackedMessages.Should().ContainSingle();

        var message = trackedMessages.Single();
        message.Type.Should().Be("user.created");
        message.Payload.Should().Be(JsonSerializer.Serialize(integrationEvent));
        message.CreatedAt.Should().BeOnOrAfter(startedAt);
        message.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task EnqueueAsync_WithMultipleIntegrationEvents_ShouldStoreAllOutboxMessages()
    {
        // Arrange
        await using var context = CreateContext();
        var loggerMock = new Mock<ILogger<OutboxWriter>>();
        var writer = new OutboxWriter(context, loggerMock.Object);
        var firstEvent = new UserCreatedIntegrationEvent(
            Guid.NewGuid(),
            "alice@example.com");
        var secondEvent = new UserActivatedIntegrationEvent(Guid.NewGuid());
        var cancellationToken = CancellationToken.None;

        // Act
        await writer.EnqueueAsync(firstEvent, cancellationToken);
        await writer.EnqueueAsync(secondEvent, cancellationToken);

        // Assert
        var trackedMessages = context.ChangeTracker
            .Entries<OutboxMessage>()
            .Select(entry => entry.Entity)
            .OrderBy(message => message.CreatedAt)
            .ToList();

        trackedMessages.Should().HaveCount(2);

        trackedMessages[0].Type.Should().Be("user.created");
        trackedMessages[0].Payload.Should().Be(JsonSerializer.Serialize(firstEvent));

        trackedMessages[1].Type.Should().Be("user.activated");
        trackedMessages[1].Payload.Should().Be(JsonSerializer.Serialize(secondEvent));
    }

    [Fact]
    public async Task EnqueueAsync_WhenDbContextThrows_ShouldPropagateException()
    {
        // Arrange
        var context = CreateContext();
        await context.DisposeAsync();

        var loggerMock = new Mock<ILogger<OutboxWriter>>();
        var writer = new OutboxWriter(context, loggerMock.Object);
        var integrationEvent = new UserCreatedIntegrationEvent(
            Guid.NewGuid(),
            "alice@example.com");
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await writer.EnqueueAsync(integrationEvent, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    private static AuthDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new AuthDbContext(options);
    }
}
