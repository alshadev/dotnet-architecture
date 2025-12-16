# dotnet-architecture
Modern dotnet architecture using .NET 10 and EF Core

## Features

This repository demonstrates a clean architecture implementation with a sample CRUD API for managing Products.

### Architecture Layers

- **Domain Layer** (`Arusha.Template.Domain`)
  - Contains entities (Product)
  - Contains repository interfaces (IProductRepository)

- **Application Layer** (`Arusha.Template.Application`)
  - Contains DTOs (ProductDto, CreateProductDto, UpdateProductDto)
  - Contains service interfaces and implementations (IProductService, ProductService)
  - Business logic layer

- **Infrastructure Layer** (`Arusha.Template.Infrastructure`)
  - Contains Entity Framework Core DbContext (ApplicationDbContext)
  - Contains repository implementations (ProductRepository)
  - Uses in-memory database for easy setup

- **API Layer** (`Arusha.Template.Api`)
  - Contains REST API controllers (ProductsController)
  - Exposes CRUD endpoints

### API Endpoints

The Products API provides the following endpoints:

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get a product by ID
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update an existing product
- `DELETE /api/products/{id}` - Delete a product

### Running the Application

```bash
cd src/Arusha.Template.Api
dotnet run
```

The API will be available at `http://localhost:5178`

### Interactive API Documentation (Swagger)

The API includes Swagger UI for interactive testing and documentation. Once the application is running, navigate to:

- **Swagger UI**: `http://localhost:5178/swagger`

This provides a user-friendly interface to:
- View all available endpoints
- Test API operations directly from the browser
- See request/response schemas
- Execute API calls without additional tools

### Example Usage

Create a product:
```bash
curl -X POST http://localhost:5178/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Laptop","description":"High performance laptop","price":1299.99,"stock":10}'
```

Get all products:
```bash
curl http://localhost:5178/api/products
```

### Running Tests

Run all tests:
```bash
dotnet test
```

Run integration tests only:
```bash
dotnet test tests/Arusha.Template.IntegrationTest
```
