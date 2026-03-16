using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Persistence.Inbox;
using TransactionService.Infrastructure.Persistence.Outbox;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public TransactionDbContext(
        DbContextOptions<TransactionDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.HasKey(x => x.TransactionId);

            builder.OwnsOne(x => x.Money, money =>
            {
                money.Property(x => x.Amount).HasColumnName("Amount");
                money.Property(x => x.Currency).HasColumnName("Currency");
                money.Property(x => x.Currency)
                .HasColumnName("Currency")
                .HasConversion<string>();  
            });
        });
        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);

            b.Property(x => x.Type).IsRequired().HasMaxLength(256);
            b.Property(x => x.RoutingKey).IsRequired().HasMaxLength(256);
            b.Property(x => x.Payload).IsRequired();

            b.Property(x => x.ReceivedAt).IsRequired();
            b.Property(x => x.AttemptCount).IsRequired();

            b.HasIndex(x => x.ProcessedAt);
            b.HasIndex(x => x.ProcessedAt);
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
