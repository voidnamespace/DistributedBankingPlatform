using FeeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Data;

public class FeeDbContext : DbContext
{
    public FeeDbContext(
        DbContextOptions<FeeDbContext> options)
        : base(options)
    {

    }

    public DbSet<UserMaintenanceFeeState> UserMaintenanceFeeStates { get; set; }

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

    }
}