namespace Arusha.Template.Application.Features.Products.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative.");

        RuleFor(x => x.Sku)
            .MaximumLength(50)
            .WithMessage("SKU cannot exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Sku));

        RuleFor(x => x.Category)
            .MaximumLength(100)
            .WithMessage("Category cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Category));
    }
}
