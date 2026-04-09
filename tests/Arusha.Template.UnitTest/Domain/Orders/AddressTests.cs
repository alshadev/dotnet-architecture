namespace Arusha.Template.UnitTest.Domain.Orders;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAddress()
    {
        var address = Address.Create("123 Main St", "Springfield", "IL", "62701", "US");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("Springfield");
        address.State.Should().Be("IL");
        address.Country.Should().Be("US");
    }

    [Fact]
    public void Create_WithMissingStreet_ShouldThrow()
    {
        var act = () => Address.Create("", "Springfield", "IL", "62701", "US");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithMissingCity_ShouldThrow()
    {
        var act = () => Address.Create("123 Main St", "", "IL", "62701", "US");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithMissingCountry_ShouldThrow()
    {
        var act = () => Address.Create("123 Main St", "City", "IL", "62701", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullState_ShouldDefaultToEmpty()
    {
        var address = Address.Create("123 Main St", "City", null, null, "US");

        address.State.Should().BeEmpty();
        address.PostalCode.Should().BeEmpty();
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var a = Address.Create("123 Main St", "City", "IL", "62701", "US");
        var b = Address.Create("123 Main St", "City", "IL", "62701", "US");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        var a = Address.Create("123 Main St", "City", "IL", "62701", "US");
        var b = Address.Create("456 Oak Ave", "City", "IL", "62701", "US");

        a.Should().NotBe(b);
    }
}
