using Arusha.Template.Application.Features.Orders.CreateOrder;
using FluentValidation.TestHelper;

namespace Arusha.Template.UnitTest.Application.Orders;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    private static CreateOrderCommand CreateValidCommand() => new(
        CustomerId: "customer-1",
        ShippingAddress: new CreateOrderCommand.AddressDto(
            "123 Main St", "Springfield", "IL", "62701", "US"),
        Items:
        [
            new CreateOrderCommand.OrderItemDto(
                Guid.NewGuid(), "Widget", 2, 25.00m, "USD")
        ]);

    [Fact]
    public async Task ValidCommand_ShouldHaveNoErrors()
    {
        var command = CreateValidCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task EmptyCustomerId_ShouldHaveError()
    {
        var command = CreateValidCommand() with { CustomerId = "" };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public async Task NullShippingAddress_ShouldHaveError()
    {
        var command = CreateValidCommand() with { ShippingAddress = null };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress);
    }

    [Fact]
    public async Task EmptyItems_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Items = [] };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public async Task ItemWithZeroQuantity_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            Items =
            [
                new CreateOrderCommand.OrderItemDto(
                    Guid.NewGuid(), "Widget", 0, 25.00m, "USD")
            ]
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public async Task ItemWithNegativePrice_ShouldHaveError()
    {
        var command = CreateValidCommand() with
        {
            Items =
            [
                new CreateOrderCommand.OrderItemDto(
                    Guid.NewGuid(), "Widget", 1, -5.00m, "USD")
            ]
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}
