using BankCardService.Domain.Entities;
using BankCardService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace BankCardService.Infrastructure.Data;

public class BankCardDbContext : DbContext
{
   

    public BankCardDbContext (DbContextOptions<BankCardDbContext> options) : base(options)
    {      
    }

    public DbSet<BankCard> BankCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankCard>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CardNumber)
                 .HasConversion(
                     x => x.Value,               
                     x => new CardNumberVO(x)    
                 )
                 .IsRequired();      

            entity.Property(x => x.CardHolder)
            .IsRequired();
            
            entity.Property(x => x.IsActive)
            .IsRequired();      

        });
    }
}
