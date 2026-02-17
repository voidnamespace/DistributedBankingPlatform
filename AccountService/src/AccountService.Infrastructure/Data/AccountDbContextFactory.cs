using AccountService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AccountDbContextFactory
    : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=accountdb;Username=postgres;Password=postgres"
            )
            .Options;

        return new AccountDbContext(options);
    }
}
