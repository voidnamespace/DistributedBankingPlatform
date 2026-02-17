using AccountService.Domain.Entity;
using AccountService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace AccountService.Infrastructure.Data;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options)
    : base(options)
    {
    }
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var accountNumberConverter = new ValueConverter<AccountNumberVO, string>(
        vo => vo.Value,
        value => new AccountNumberVO(value));

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
    }
}
