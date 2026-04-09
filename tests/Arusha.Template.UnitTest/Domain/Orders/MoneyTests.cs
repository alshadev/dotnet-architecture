namespace Arusha.Template.UnitTest.Domain.Orders;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        var money = Money.Create(100.50m, "USD");

        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Money.Create(-1, "USD");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldNormalizeCurrencyToUpperCase()
    {
        var money = Money.Create(10, "usd");

        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var a = Money.Create(10, "USD");
        var b = Money.Create(20, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(30);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var usd = Money.Create(10, "USD");
        var eur = Money.Create(20, "EUR");

        var act = () => usd.Add(eur);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_ShouldReturnDifference()
    {
        var a = Money.Create(30, "USD");
        var b = Money.Create(10, "USD");

        var result = a.Subtract(b);

        result.Amount.Should().Be(20);
    }

    [Fact]
    public void Subtract_WhenResultNegative_ShouldThrow()
    {
        var a = Money.Create(5, "USD");
        var b = Money.Create(10, "USD");

        var act = () => a.Subtract(b);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_ShouldReturnProduct()
    {
        var money = Money.Create(10, "USD");

        var result = money.Multiply(3);

        result.Amount.Should().Be(30);
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var a = Money.Create(10, "USD");
        var b = Money.Create(10, "USD");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = Money.Create(10, "USD");
        var b = Money.Create(20, "USD");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        var zero = Money.Zero("USD");

        zero.Amount.Should().Be(0);
        zero.Currency.Should().Be("USD");
    }
}
