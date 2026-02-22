using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Persistence.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    private readonly IMediator? _mediator;

    public AuthDbContext(
    DbContextOptions<AuthDbContext> options,
    IMediator? mediator = null)
    : base(options)
    {
        _mediator = mediator;
    }

    public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        if (_mediator is null)
            return;

        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>)
                .MakeGenericType(domainEvent.GetType());

            var notification = Activator.CreateInstance(
                notificationType,
                domainEvent
            ) as INotification;

            if (notification is not null)
            {
                await _mediator.Publish(notification, ct);
            }
        }
    }
    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Email)
                 .HasConversion(
                    email => email.Value,
                   str => new EmailVO(str))
                  .HasColumnName("Email")
                  .HasMaxLength(255)
                  .IsRequired();


            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasConversion(
                    pwd => pwd != null ? pwd.Hash : "",
                    hash => PasswordVO.FromHash(hash))
                .HasColumnName("PasswordHash")
                .IsRequired();

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasDefaultValue(Roles.Customer)
                .IsRequired();

            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.Property(e => e.ExpiryDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type).IsRequired().HasMaxLength(256);
            b.Property(x => x.RoutingKey).IsRequired().HasMaxLength(256);
            b.Property(x => x.Payload).IsRequired();

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.AttemptCount).IsRequired();

            b.HasIndex(x => x.ProcessedAt);
            b.HasIndex(x => x.CreatedAt);
        });
    }
}
