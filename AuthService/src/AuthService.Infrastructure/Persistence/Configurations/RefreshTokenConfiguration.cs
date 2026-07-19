using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(refreshToken => refreshToken.Token)
            .IsUnique();

        builder.Property(refreshToken => refreshToken.ExpiryDate)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedAt)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.IsRevoked)
            .IsConcurrencyToken()
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(refreshToken => refreshToken.UserId)
            .IsRequired();
    }
}
