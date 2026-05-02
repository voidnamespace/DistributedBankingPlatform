using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Infrastructure.Inbox;

namespace UserSegmentationService.Infrastructure.Persistence.Database;

public class SegmentationDbContext : DbContext
{
    public SegmentationDbContext(
        DbContextOptions<SegmentationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserMetric> UserMetrics { get; set; }

    public DbSet<UserAccount> UserAccounts { get; set; }

    public DbSet<InboxMessage> InboxMessages { get; set; }

    public DbSet<DeadLetterInboxMessage> DeadLetterInboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserMetric>(entity =>
        {
            entity.HasKey(x => x.UserId);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(x => x.AccountId);

            entity.Property(x => x.AccountNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.AccountNumber)
                .IsUnique();

            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(x => x.MessageId);

            entity.Property(x => x.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.LastError)
                .HasMaxLength(4000);

            entity.HasIndex(x => new { x.ProcessedAt, x.NextAttemptAt });
        });

        modelBuilder.Entity<DeadLetterInboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.Error)
                .HasMaxLength(4000)
                .IsRequired();

            entity.HasIndex(x => x.MessageId);
            entity.HasIndex(x => x.FailedAt);
        });
    }
}
