namespace Arusha.Template.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OrderItem entity.
/// </summary>
internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        // Strongly typed ID conversion
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => OrderItemId.From(value))
            .HasColumnName("Id");

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        // Value object as owned entity
        builder.OwnsOne(i => i.UnitPrice, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property(p => p.Currency)
                .HasMaxLength(3)
                .HasColumnName("Currency")
                .IsRequired();
        });

        // Ignore calculated property
        builder.Ignore(i => i.TotalPrice);
    }
}
