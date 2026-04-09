namespace Arusha.Template.UnitTest.Domain.Orders;

public class OrderStatusTests
{
    [Theory]
    [InlineData(1, 2, true)]  // Pending -> Confirmed
    [InlineData(1, 5, true)]  // Pending -> Cancelled
    [InlineData(2, 3, true)]  // Confirmed -> Shipped
    [InlineData(2, 5, true)]  // Confirmed -> Cancelled
    [InlineData(3, 4, true)]  // Shipped -> Delivered
    public void CanTransitionTo_ValidTransitions_ShouldReturnTrue(int fromValue, int toValue, bool expected)
    {
        var from = Enumeration<OrderStatus>.FromValue(fromValue);
        var to = Enumeration<OrderStatus>.FromValue(toValue);

        from.CanTransitionTo(to).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 1)]  // Pending -> Pending (same)
    [InlineData(1, 3)]  // Pending -> Shipped (skip)
    [InlineData(2, 1)]  // Confirmed -> Pending (back)
    [InlineData(4, 1)]  // Delivered -> anything
    [InlineData(5, 1)]  // Cancelled -> anything
    [InlineData(3, 2)]  // Shipped -> Confirmed (back)
    public void CanTransitionTo_InvalidTransitions_ShouldReturnFalse(int fromValue, int toValue)
    {
        var from = Enumeration<OrderStatus>.FromValue(fromValue);
        var to = Enumeration<OrderStatus>.FromValue(toValue);

        from.CanTransitionTo(to).Should().BeFalse();
    }

    [Fact]
    public void FromName_ShouldResolveCorrectly()
    {
        var status = Enumeration<OrderStatus>.FromName("Pending");

        status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void GetAll_ShouldReturnAllStatuses()
    {
        var all = Enumeration<OrderStatus>.GetAll();

        all.Should().HaveCount(5);
    }
}
