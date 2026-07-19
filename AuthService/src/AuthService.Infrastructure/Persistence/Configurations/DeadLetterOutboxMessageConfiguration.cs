using AuthService.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Persistence.Configurations;

internal sealed class DeadLetterOutboxMessageConfiguration
    : IEntityTypeConfiguration<DeadLetterOutboxMessage>
{
    public void Configure(
        EntityTypeBuilder<DeadLetterOutboxMessage> builder)
    {
        builder.ToTable("DeadLetterOutboxMessages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.OriginalOutboxMessageId)
            .IsRequired();

        builder.Property(message => message.Type)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(message => message.Payload)
            .IsRequired();

        builder.Property(message => message.Error)
            .IsRequired();

        builder.Property(message => message.AttemptCount)
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .IsRequired();

        builder.Property(message => message.FinalFailedAt)
            .IsRequired();

        builder.HasIndex(message => message.OriginalOutboxMessageId)
            .IsUnique();

        builder.HasIndex(message => message.FinalFailedAt);
    }
}
