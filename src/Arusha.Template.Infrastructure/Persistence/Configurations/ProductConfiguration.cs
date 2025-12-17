namespace Arusha.Template.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Product entity.
/// </summary>
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // Strongly typed ID conversion
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ProductId.From(value))
            .HasColumnName("Id");

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Sku)
            .HasMaxLength(50);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        // Audit fields
        builder.Property(p => p.CreatedOnUtc)
            .IsRequired();

        builder.Property(p => p.ModifiedOnUtc);

        // Indexes
        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasFilter("[Sku] IS NOT NULL");

        builder.HasIndex(p => p.Category);

        builder.HasIndex(p => p.IsActive);
    }
}
