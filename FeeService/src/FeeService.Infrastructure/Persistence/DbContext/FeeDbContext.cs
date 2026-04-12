using FeeService.Domain.Entities;
using FeeService.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Persistence.DbContext;

public class FeeDbContext : DbContext
{
    public FeeDbContext(
        DbContextOptions<FeeDbContext> options)
        : base(options)
    {

    }

    public DbSet<UserMaintenanceFeeState> UserMaintenanceFeeStates { get; set; }

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserMaintenanceFeeState>(entity =>
        {
            entity.HasKey(w => w.UserId);

            entity.Property(w => w.ChargedAt)
                  .IsRequired(false);
            entity.Property(w => w.NextChargeAt)
                  .IsRequired();

            entity.Property(w => w.CreatedAt)
                  .IsRequired();

        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Payload)
                .IsRequired();

            entity.Property(x => x.Processed)
                .IsRequired();

            entity.Property(x => x.ReceivedAt)
                .IsRequired();

            entity.Property(x => x.AttemptCount)
                .IsRequired();

            entity.Property(x => x.Error)
                .HasMaxLength(2000);
        });

    }
}
