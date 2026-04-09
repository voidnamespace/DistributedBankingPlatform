using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FeeService.Infrastructure.Data;


public class FeeDbContextFactory
    : IDesignTimeDbContextFactory<FeeDbContext>
{
    public FeeDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FeeDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5435;Database=feedb;Username=postgres;Password=postgres"
            )
            .Options;

        return new FeeDbContext(options);
    }
}
