using Arusha.Template.Application.Abstractions.Persistence;
using Arusha.Template.Application.Features.Orders.CreateOrder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arusha.Template.UnitTest.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();

    private CreateOrderCommandHandler CreateHandler() =>
        new(_orderRepository, NullLogger<CreateOrderCommandHandler>.Instance);

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
    public async Task Handle_ValidCommand_ShouldReturnSuccessWithOrderId()
    {
        var handler = CreateHandler();
        var command = CreateValidCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddOrderToRepository()
    {
        var handler = CreateHandler();
        var command = CreateValidCommand();

        await handler.Handle(command, CancellationToken.None);

        _orderRepository.Received(1).Add(Arg.Is<Order>(o =>
            o.CustomerId == "customer-1" &&
            o.Items.Count == 1));
    }

    [Fact]
    public async Task Handle_MultipleItems_ShouldAddAllItems()
    {
        var handler = CreateHandler();
        var command = new CreateOrderCommand(
            CustomerId: "customer-1",
            ShippingAddress: new CreateOrderCommand.AddressDto(
                "123 Main St", "Springfield", "IL", "62701", "US"),
            Items:
            [
                new CreateOrderCommand.OrderItemDto(Guid.NewGuid(), "Widget A", 1, 10.00m, "USD"),
                new CreateOrderCommand.OrderItemDto(Guid.NewGuid(), "Widget B", 3, 20.00m, "USD")
            ]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _orderRepository.Received(1).Add(Arg.Is<Order>(o => o.Items.Count == 2));
    }
}
