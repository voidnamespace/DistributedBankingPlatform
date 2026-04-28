using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Domain.Entities;

namespace UserSegmentationService.Infrastructure.Persistence.Database;

public class SegmentationDbContext : DbContext
{

    public SegmentationDbContext(
        DbContextOptions<SegmentationDbContext> options)
        : base(options)
    {

    }

    public DbSet<UserMetric> UserMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserMetric>(entity =>
        {
            entity.HasKey(x => x.UserId);
        });

    }

    }
