using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace AuthService.Infrastructure.Persistence.Seeding;

public class AuthDbSeeder
{
    private readonly AuthDbContext _context;

    public AuthDbSeeder(AuthDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var adminExists = await _context.Users
            .AnyAsync(u => u.Role == Roles.Admin);

        if (adminExists)
            return;

        var admin = User.CreateAdmin(
            EmailVO.Create("admin@local.dev"),
            PasswordVO.Create("admin123")
        );

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();
    }
}
