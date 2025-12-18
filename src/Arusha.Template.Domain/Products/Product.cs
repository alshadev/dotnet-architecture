namespace Arusha.Template.Domain.Products;

/// <summary>
/// Product entity demonstrating Pragmatic DDD.
/// 
/// Unlike Order (rich domain model), Product is a simpler entity
/// suitable for CRUD operations. This demonstrates that not all
/// entities need complex domain logic.
/// 
/// Key differences from Order:
/// - Simple entity, not aggregate root (no child entities)
/// - No domain events (simple CRUD)
/// - Direct property setters with basic validation
/// - Simpler factory method
/// 
/// This is "pragmatic" because we apply DDD tactically where it
/// provides value, not dogmatically everywhere.
/// </summary>
public sealed class Product : Entity<ProductId>, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = default!;
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public string Sku { get; private set; }
    public string Category { get; private set; }

    /// <summary>
    /// Auditable properties.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string ModifiedBy { get; set; }

    /// <summary>
    /// Soft-delete properties.
    /// </summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    public string DeletedBy { get; set; }

    private Product(
        ProductId id,
        string name,
        string description,
        decimal price,
        string currency,
        int stockQuantity,
        string sku,
        string category)
        : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        Currency = currency;
        StockQuantity = stockQuantity;
        Sku = sku;
        Category = category;
        IsActive = true;
    }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Product() : base() { }

    /// <summary>
    /// Factory method to create a new product.
    /// </summary>
    public static Product Create(
        string name,
        string description,
        decimal price,
        string currency = "USD",
        int stockQuantity = 0,
        string sku = null,
        string category = null)
    {
        ValidateName(name);
        ValidatePrice(price);
        ValidateStockQuantity(stockQuantity);

        return new Product(
            ProductId.New(),
            name,
            description ?? string.Empty,
            price,
            currency,
            stockQuantity,
            sku,
            category);
    }

    /// <summary>
    /// Updates the product details.
    /// </summary>
    public void Update(
        string name,
        string description,
        decimal price,
        string currency,
        string sku,
        string category)
    {
        ValidateName(name);
        ValidatePrice(price);

        Name = name;
        Description = description ?? string.Empty;
        Price = price;
        Currency = currency;
        Sku = sku;
        Category = category;
    }

    /// <summary>
    /// Updates the stock quantity.
    /// </summary>
    public void UpdateStock(int quantity)
    {
        ValidateStockQuantity(quantity);
        StockQuantity = quantity;
    }

    /// <summary>
    /// Adds stock to the current quantity.
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        StockQuantity += quantity;
    }

    /// <summary>
    /// Removes stock from the current quantity.
    /// </summary>
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        if (StockQuantity < quantity)
            throw new InvalidOperationException("Not enough stock available.");

        StockQuantity -= quantity;
    }

    /// <summary>
    /// Deactivates the product.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the product.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Checks if the product has enough stock.
    /// </summary>
    public bool HasStock(int quantity) => StockQuantity >= quantity;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters.", nameof(name));
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));
    }

    private static void ValidateStockQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(quantity));
    }
}
