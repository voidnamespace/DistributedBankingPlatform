using AccountService.Domain.Entity;
using AccountService.Domain.ValueObjects;
using AccountService.Infrastructure.Persistence.Inbox;
using AccountService.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace AccountService.Infrastructure.Data;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options)
    : base(options)
    {
    }
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterInboxMessage> DeadLetterInboxMessages => Set<DeadLetterInboxMessage>();
    public DbSet<DeadLetterOutboxMessage> DeadLetterOutboxMessages => Set<DeadLetterOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var accountNumberConverter = new ValueConverter<AccountNumberVO, string>(
        vo => vo.Value,
        value => new AccountNumberVO(value));
        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("InboxMessages"); 

            b.HasKey(x => x.Id);

            b.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.Payload)
                .IsRequired();

            b.Property(x => x.ReceivedAt)
                .IsRequired();

            b.Property(x => x.ProcessedAt);

            b.Property(x => x.AttemptCount)
                .IsRequired();

            b.HasIndex(x => x.ProcessedAt);

            b.Property(x => x.TraceParent);
            b.Property(x => x.TraceState);
        });
        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");

            b.HasKey(x => x.Id);

            b.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.Payload)
                .IsRequired();

            b.Property(x => x.OccurredOnUtc)
                .IsRequired();

            b.Property(x => x.ProcessedOnUtc);

            b.Property(x => x.AttemptCount)
                .IsRequired();

            b.Property(x => x.Error);

            b.HasIndex(x => x.ProcessedOnUtc);
            b.HasIndex(x => x.OccurredOnUtc);

            b.Property(x => x.TraceParent);
            b.Property(x => x.TraceState);
        });
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.UserId)
            .IsRequired();

            entity.HasIndex(u => u.AccountNumber)
            .IsUnique();
            entity.Property(u => u.AccountNumber)
            .HasConversion(accountNumberConverter)
            .IsRequired()
            .HasMaxLength(12);

            entity.OwnsOne(a => a.Balance, balance =>
            {
                balance.Property(b => b.Amount)
                    .HasColumnName("BalanceAmount")
                    .IsRequired();

                balance.Property(b => b.Currency)
                    .HasColumnName("BalanceCurrency")
                    .IsRequired();
            });

            entity.Property(u => u.CreatedAt)
            .IsRequired();

            entity.Property(u => u.UpdatedAt)
            .IsRequired();

            entity.Property(u => u.IsActive)
            .HasDefaultValue(true);

            entity.Property(a => a.RowVersion)
            .IsRowVersion();

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
                .IsRequired();

            entity.HasIndex(x => x.MessageId);
            entity.HasIndex(x => x.FailedAt);
        });

        modelBuilder.Entity<DeadLetterOutboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(x => x.Error)
                .IsRequired();

            entity.HasIndex(x => x.MessageId);
            entity.HasIndex(x => x.FailedAt);
        });
    }
}
