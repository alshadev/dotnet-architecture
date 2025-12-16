using System.Net;
using System.Net.Http.Json;
using Arusha.Template.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Arusha.Template.IntegrationTest;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoProducts()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task Create_ReturnsCreatedProduct()
    {
        var newProduct = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        var response = await _client.PostAsJsonAsync("/api/products", newProduct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(createdProduct);
        Assert.Equal(newProduct.Name, createdProduct.Name);
        Assert.Equal(newProduct.Description, createdProduct.Description);
        Assert.Equal(newProduct.Price, createdProduct.Price);
        Assert.Equal(newProduct.Stock, createdProduct.Stock);
        Assert.True(createdProduct.Id > 0);
    }

    [Fact]
    public async Task GetById_ReturnsProduct_WhenProductExists()
    {
        var newProduct = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await _client.GetAsync($"/api/products/{createdProduct!.Id}");
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal(createdProduct.Id, product.Id);
        Assert.Equal(createdProduct.Name, product.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var response = await _client.GetAsync("/api/products/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsUpdatedProduct_WhenProductExists()
    {
        var newProduct = new CreateProductDto
        {
            Name = "Original Product",
            Description = "Original Description",
            Price = 50.00m,
            Stock = 5
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var updateProduct = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 75.00m,
            Stock = 15
        };

        var response = await _client.PutAsJsonAsync($"/api/products/{createdProduct!.Id}", updateProduct);
        response.EnsureSuccessStatusCode();

        var updatedProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(updatedProduct);
        Assert.Equal(createdProduct.Id, updatedProduct.Id);
        Assert.Equal(updateProduct.Name, updatedProduct.Name);
        Assert.Equal(updateProduct.Description, updatedProduct.Description);
        Assert.Equal(updateProduct.Price, updatedProduct.Price);
        Assert.Equal(updateProduct.Stock, updatedProduct.Stock);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var updateProduct = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 75.00m,
            Stock = 15
        };

        var response = await _client.PutAsJsonAsync("/api/products/99999", updateProduct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenProductExists()
    {
        var newProduct = new CreateProductDto
        {
            Name = "Product To Delete",
            Description = "Will be deleted",
            Price = 25.00m,
            Stock = 3
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        var response = await _client.DeleteAsync($"/api/products/{createdProduct!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenProductDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/products/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
