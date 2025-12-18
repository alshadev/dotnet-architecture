namespace Arusha.Template.Application.Features.Orders.CreateOrder;

/// <summary>
/// Validator for CreateOrderCommand.
/// Uses FluentValidation for declarative validation rules.
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required.");

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("Shipping address is required.");

        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress.Street)
                .NotEmpty()
                .WithMessage("Street is required.")
                .MaximumLength(200)
                .WithMessage("Street cannot exceed 200 characters.");

            RuleFor(x => x.ShippingAddress.City)
                .NotEmpty()
                .WithMessage("City is required.")
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters.");

            RuleFor(x => x.ShippingAddress.Country)
                .NotEmpty()
                .WithMessage("Country is required.")
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters.");
        });

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required.");

            item.RuleFor(x => x.ProductName)
                .NotEmpty()
                .WithMessage("Product name is required.")
                .MaximumLength(200)
                .WithMessage("Product name cannot exceed 200 characters.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit price cannot be negative.");
        });
    }
}
