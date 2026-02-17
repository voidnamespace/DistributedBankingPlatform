using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TransactionService.Infrastructure.Data;

public class TransactionDbContextFactory
    : IDesignTimeDbContextFactory<TransactionDbContext>
{
    public TransactionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<TransactionDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=transactiondb;Username=postgres;Password=postgres");

        return new TransactionDbContext(optionsBuilder.Options);
    }
}
