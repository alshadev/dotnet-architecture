namespace Arusha.Template.Application.Features.Orders.GetOrder;

/// <summary>
/// Query to get an order by ID.
/// </summary>
public sealed record GetOrderQuery(Guid OrderId) : IQuery<OrderResponse>;

/// <summary>
/// Response DTO for order queries.
/// </summary>
public sealed record OrderResponse(
    Guid Id,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    AddressResponse ShippingAddress,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt);

public sealed record AddressResponse(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Currency);
