using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankCardService.Infrastructure.Data;

public class BankCardDbContextFactory : IDesignTimeDbContextFactory<BankCardDbContext>
{
    public BankCardDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BankCardDbContext>();

        optionsBuilder.UseNpgsql(
    "Host=localhost;Port=5432;Database=bankcard;Username=postgres;Password=Wrigdaemm1n");

        return new BankCardDbContext(optionsBuilder.Options);
    }
}
