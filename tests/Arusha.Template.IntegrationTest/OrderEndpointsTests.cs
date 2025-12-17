namespace Arusha.Template.IntegrationTest;

public class OrderEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task Create_and_get_order_successfully()
    {
        var command = new CreateOrderCommand(
            "customer-1",
            new CreateOrderCommand.AddressDto("123 Street", "City", "ST", "12345", "US"),
            [
                new CreateOrderCommand.OrderItemDto(Guid.NewGuid(), "Test Product", 2, 10m, "USD")
            ]);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", command);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdEnvelope = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        createdEnvelope.Should().NotBeNull();
        createdEnvelope!.Success.Should().BeTrue();
        createdEnvelope.Data.Should().NotBe(Guid.Empty);

        var getResponse = await _client.GetAsync($"/api/v1/orders/{createdEnvelope.Data}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderEnvelope = await getResponse.Content.ReadFromJsonAsync<ApiResponse<OrderResponse>>();

        orderEnvelope.Should().NotBeNull();
        orderEnvelope!.Success.Should().BeTrue();
        orderEnvelope.Data!.Id.Should().Be(createdEnvelope.Data);
        orderEnvelope.Data.Items.Should().HaveCount(1);
    }
}
