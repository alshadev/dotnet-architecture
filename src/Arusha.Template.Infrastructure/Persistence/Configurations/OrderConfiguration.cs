namespace Arusha.Template.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Order aggregate.
/// Demonstrates:
/// - Strongly typed ID conversion
/// - Value object mapping (owned entities)
/// - One-to-many relationship
/// - Smart enumeration conversion
/// </summary>
internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // Strongly typed ID conversion
        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.From(value))
            .HasColumnName("Id");

        builder.Property(o => o.CustomerId)
            .HasMaxLength(100)
            .IsRequired();

        // Value object as owned entity
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(200)
                .HasColumnName("ShippingStreet")
                .IsRequired();

            address.Property(a => a.City)
                .HasMaxLength(100)
                .HasColumnName("ShippingCity")
                .IsRequired();

            address.Property(a => a.State)
                .HasMaxLength(100)
                .HasColumnName("ShippingState");

            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("ShippingPostalCode");

            address.Property(a => a.Country)
                .HasMaxLength(100)
                .HasColumnName("ShippingCountry")
                .IsRequired();
        });

        // Smart enumeration conversion
        builder.Property(o => o.Status)
            .HasConversion(
                status => status.Value,
                value => OrderStatus.FromValue(value)!)
            .HasColumnName("StatusId")
            .IsRequired();

        builder.Property(o => o.ConfirmedAt);
        builder.Property(o => o.ShippedAt);
        builder.Property(o => o.DeliveredAt);
        builder.Property(o => o.CancelledAt);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(o => o.CreatedOnUtc)
            .IsRequired();

        builder.Property(o => o.ModifiedOnUtc);

        // Relationship with OrderItems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore calculated property
        builder.Ignore(o => o.TotalPrice);
    }
}
