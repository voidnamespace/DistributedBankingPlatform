using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

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
            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.Money, money =>
            {
                money.Property(x => x.Amount).HasColumnName("Amount");
                money.Property(x => x.Currency).HasColumnName("Currency");
                money.Property(x => x.Currency)
                .HasColumnName("Currency")
                .HasConversion<string>();  
            });
        });       
    }
}
