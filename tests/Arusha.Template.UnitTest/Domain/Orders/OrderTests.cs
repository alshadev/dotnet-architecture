namespace Arusha.Template.UnitTest.Domain.Orders;

public class OrderTests
{
    private static Address CreateTestAddress() =>
        Address.Create("123 Main St", "Springfield", "IL", "62701", "US");

    private static Money CreateTestPrice(decimal amount = 25.00m) =>
        Money.Create(amount, "USD");

    [Fact]
    public void Create_ShouldSetPendingStatus()
    {
        var order = Order.Create("customer-1", CreateTestAddress());

        order.Status.Should().Be(OrderStatus.Pending);
        order.CustomerId.Should().Be("customer-1");
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var order = Order.Create("customer-1", CreateTestAddress());

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>();
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrow()
    {
        var act = () => Order.Create("", CreateTestAddress());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_ShouldAddToCollection()
    {
        var order = Order.Create("customer-1", CreateTestAddress());

        order.AddItem(Guid.NewGuid(), "Widget", 2, CreateTestPrice());

        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_DuplicateProduct_ShouldMergeQuantity()
    {
        var order = Order.Create("customer-1", CreateTestAddress());
        var productId = Guid.NewGuid();

        order.AddItem(productId, "Widget", 2, CreateTestPrice());
        order.AddItem(productId, "Widget", 3, CreateTestPrice());

        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void TotalPrice_ShouldSumAllItems()
    {
        var order = Order.Create("customer-1", CreateTestAddress());
        order.AddItem(Guid.NewGuid(), "Widget A", 2, Money.Create(10, "USD"));
        order.AddItem(Guid.NewGuid(), "Widget B", 1, Money.Create(30, "USD"));

        // 2 * 10 + 1 * 30 = 50
        order.TotalPrice.Amount.Should().Be(50);
    }

    [Fact]
    public void Confirm_WithItems_ShouldTransitionToConfirmed()
    {
        var order = Order.Create("customer-1", CreateTestAddress());
        order.AddItem(Guid.NewGuid(), "Widget", 1, CreateTestPrice());

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_WithNoItems_ShouldThrow()
    {
        var order = Order.Create("customer-1", CreateTestAddress());

        var act = () => order.Confirm();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*without items*");
    }

    [Fact]
    public void Confirm_ShouldRaiseIntegrationEvent()
    {
        var order = Order.Create("customer-1", CreateTestAddress());
        order.AddItem(Guid.NewGuid(), "Widget", 1, CreateTestPrice());

        order.Confirm();

        order.DomainEvents.Should().Contain(e =>
            e is OrderConfirmedDomainEvent);
    }

    [Fact]
    public void Ship_FromConfirmed_ShouldTransitionToShipped()
    {
        var order = CreateConfirmedOrder();

        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();
    }

    [Fact]
    public void Ship_FromPending_ShouldThrow()
    {
        var order = Order.Create("customer-1", CreateTestAddress());
        order.AddItem(Guid.NewGuid(), "Widget", 1, CreateTestPrice());

        var act = () => order.Ship();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_FromPending_ShouldSucceed()
    {
        var order = Order.Create("customer-1", CreateTestAddress());

        order.Cancel("Changed my mind");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Changed my mind");
    }

    [Fact]
    public void Cancel_FromDelivered_ShouldThrow()
    {
        var order = CreateDeliveredOrder();

        var act = () => order.Cancel("Too late");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddItem_WhenNotPending_ShouldThrow()
    {
        var order = CreateConfirmedOrder();

        var act = () => order.AddItem(Guid.NewGuid(), "Widget", 1, CreateTestPrice());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot modify*");
    }

    private static Order CreateConfirmedOrder()
    {
        var order = Order.Create("customer-1", CreateTestAddress());
        order.AddItem(Guid.NewGuid(), "Widget", 1, CreateTestPrice());
        order.Confirm();
        return order;
    }

    private static Order CreateDeliveredOrder()
    {
        var order = CreateConfirmedOrder();
        order.Ship();
        order.Deliver();
        return order;
    }
}
