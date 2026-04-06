using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=authdb;Username=postgres;Password=postgres");

        return new AuthDbContext(optionsBuilder.Options);
    }
}
