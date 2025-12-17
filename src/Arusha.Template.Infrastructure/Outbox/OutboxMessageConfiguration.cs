namespace Arusha.Template.Infrastructure.Outbox;

/// <summary>
/// EF Core configuration for OutboxMessage.
/// </summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.OccurredOnUtc)
            .IsRequired();

        builder.Property(m => m.ProcessedOnUtc);

        builder.Property(m => m.Error)
            .HasMaxLength(2000);

        builder.Property(m => m.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Index for efficient polling of unprocessed messages
        builder.HasIndex(m => new { m.ProcessedOnUtc, m.OccurredOnUtc })
            .HasFilter("[ProcessedOnUtc] IS NULL");
    }
}
