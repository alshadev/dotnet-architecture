namespace Arusha.Template.Application.Features.Orders.ConfirmOrder;

/// <summary>
/// Command to confirm an order.
/// </summary>
public sealed record ConfirmOrderCommand(Guid OrderId) : ICommand;
