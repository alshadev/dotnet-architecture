namespace Arusha.Template.Infrastructure.Idempotency;

/// <summary>
/// EF Core configuration for IdempotentRequest.
/// </summary>
internal sealed class IdempotentRequestConfiguration : IEntityTypeConfiguration<IdempotentRequest>
{
    public void Configure(EntityTypeBuilder<IdempotentRequest> builder)
    {
        builder.ToTable("IdempotentRequests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.CreatedOnUtc)
            .IsRequired();

        // Index for cleanup of old records
        builder.HasIndex(r => r.CreatedOnUtc);
    }
}
