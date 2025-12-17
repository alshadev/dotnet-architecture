namespace Arusha.Template.Application.Features.Orders.CreateOrder;

/// <summary>
/// Command to create a new order.
/// Demonstrates CQRS command pattern with nested DTOs.
/// </summary>
public sealed record CreateOrderCommand(
    string CustomerId,
    CreateOrderCommand.AddressDto ShippingAddress,
    List<CreateOrderCommand.OrderItemDto> Items) : ICommand<Guid>
{
    public sealed record AddressDto(
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country);

    public sealed record OrderItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        string Currency = "USD");
}
