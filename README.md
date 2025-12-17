# Arusha Template - .NET Clean Architecture

A .NET 10 template implementing **Clean Architecture**, **Vertical Slice Architecture**, and **Pragmatic Domain-Driven Design**.

---

## Why This Architecture?

### The Problem with Traditional Layered Architecture

Traditional N-tier architecture (Controller → Service → Repository) creates several issues:

- **Tight coupling** - Business logic scattered across services, hard to change
- **Anemic domain models** - Entities become data bags, logic lives in services
- **Feature fragmentation** - Adding a feature requires touching multiple folders
- **Testing complexity** - Services depend on other services, mocking becomes painful

### Clean Architecture as the Solution

Clean Architecture solves these by **inverting dependencies** - the domain is at the center, infrastructure depends on domain (not vice versa).

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                                │
│           Endpoints • Middleware • Configuration                 │
└────────────────────────────────┬────────────────────────────────┘
                                 │ depends on
┌────────────────────────────────▼────────────────────────────────┐
│                     Application Layer                            │
│         Use Cases • CQRS • Behaviors • Validation               │
└────────────────────────────────┬────────────────────────────────┘
                                 │ depends on
┌────────────────────────────────▼────────────────────────────────┐
│                       Domain Layer                               │
│     Entities • Value Objects • Events • Repository Interfaces   │
└────────────────────────────────▲────────────────────────────────┘
                                 │ implements
┌────────────────────────────────┴────────────────────────────────┐
│                    Infrastructure Layer                          │
│          EF Core • Repositories • Outbox • Event Bus            │
└─────────────────────────────────────────────────────────────────┘
```

**Key insight**: Application layer defines persistence abstractions (e.g., `IOrderRepository`, `IUnitOfWork`), Infrastructure implements them. Domain contains only core business logic and never depends on persistence concerns or external technologies.

---

## Why Vertical Slice Architecture?

### The Problem with Horizontal Layers

In traditional architecture, a single feature is split across layers:

```
Controllers/
  OrderController.cs      ← Part 1 of "Create Order"
Services/
  OrderService.cs         ← Part 2 of "Create Order"
Repositories/
  OrderRepository.cs      ← Part 3 of "Create Order"
DTOs/
  CreateOrderDto.cs       ← Part 4 of "Create Order"
```

**Problems**:
- Changing one feature requires navigating 4+ folders
- High coupling between features sharing the same service
- Difficult to understand a feature's full scope

### Vertical Slices as the Solution

Each feature is a self-contained unit:

```
Features/
  Orders/
    CreateOrder/
      CreateOrderCommand.cs       ← Request
      CreateOrderCommandHandler.cs ← Logic
      CreateOrderValidator.cs     ← Validation
    GetOrder/
      GetOrderQuery.cs
      GetOrderQueryHandler.cs
```

**Benefits**:
- **Feature cohesion** - Everything about "Create Order" is in one folder
- **Low coupling** - Features don't share code, changes are isolated
- **Easy onboarding** - New developers understand features quickly
- **Delete-friendly** - Remove a feature by deleting its folder

---

## Why Pragmatic DDD?

### The Problem with Dogmatic DDD

Applying full DDD patterns everywhere leads to:
- **Over-engineering** - Simple CRUD wrapped in aggregates and value objects
- **Increased complexity** - More code to maintain without business value
- **Slower development** - Pattern overhead for trivial operations

### Pragmatic Approach

Apply DDD tactically - use rich domain models where complexity exists, simple models where it doesn't.

**Order (Rich Domain Model)**:
- Complex business rules (status transitions, item management)
- Multiple invariants (can't confirm empty order)
- Domain events for side effects
- Value objects for composite values (Money, Address)

**Product (Simple Entity)**:
- Basic CRUD operations
- Minimal business rules
- No domain events needed
- Direct property updates

This demonstrates that **not every entity needs to be an aggregate**. Apply patterns where they provide value.

---

## Why CQRS?

### The Problem

A single model serving both reads and writes creates tension:
- **Reads** want flat, denormalized data for display
- **Writes** want rich domain objects with business logic
- One model can't optimize for both

### CQRS Solution

Separate the read and write paths:

```csharp
// Command - optimized for writes, goes through domain
public record CreateOrderCommand(string CustomerId, List<OrderItemDto> Items) 
    : ICommand<OrderId>;

// Query - optimized for reads, can bypass domain
public record GetOrderQuery(Guid OrderId) 
    : IQuery<OrderResponse>;
```

**Benefits**:
- Commands enforce business rules through domain
- Queries can be optimized (projections, raw SQL, read replicas)
- Clear separation of concerns
- Easier to scale independently

---

## Why Custom Result Pattern?

### The Problem with Exceptions

Using exceptions for business logic failures:
```csharp
public Order GetOrder(Guid id)
{
    var order = _repository.GetById(id);
    if (order == null)
        throw new NotFoundException("Order not found"); // Flow control via exception
    return order;
}
```

**Problems**:
- Exceptions are expensive (stack trace capture)
- Non-obvious flow control
- Easy to forget try-catch
- Return type doesn't communicate failure possibility

### Result Pattern Solution

```csharp
public Result<Order> GetOrder(Guid id)
{
    var order = _repository.GetById(id);
    if (order == null)
        return Result.Failure<Order>(OrderErrors.NotFound(id));
    return Result.Success(order);
}
```

**Benefits**:
- **Explicit** - Method signature shows it can fail
- **Type-safe** - Compiler ensures you handle both cases
- **Performant** - No exception overhead
- **Composable** - Chain operations with Map/Bind

**When to use exceptions**: Infrastructure failures (database down, network errors) - things that shouldn't happen in normal flow.

---

## Why Hybrid Domain Events?

### The Problem

Domain events have two conflicting needs:

1. **Immediate consistency** - Update related data in same transaction
2. **Reliable delivery** - Ensure events reach external systems

### Hybrid Solution

**Immediate Events (`IImmediateDomainEvent`)**:
- Dispatched before `SaveChanges()`
- Run in same transaction
- Use for: Cascading updates within bounded context

**Integration Events (`IIntegrationEvent`)**:
- Written to Outbox table during `SaveChanges()`
- Published asynchronously by background processor
- Use for: Cross-service communication, external systems

```csharp
// Same transaction - update inventory when order created
public record OrderCreatedDomainEvent(OrderId OrderId) : IImmediateDomainEvent;

// Via outbox - notify shipping service
public record OrderConfirmedIntegrationEvent(OrderId OrderId, ...) : IIntegrationEvent;
```

**Why Outbox?** Solves the dual-write problem - you can't reliably write to database AND publish to message broker atomically. Outbox ensures both succeed or both fail.

---

## Why Transactional Outbox?

### The Dual-Write Problem

```csharp
await _dbContext.SaveChangesAsync();        // ✓ Succeeds
await _messageBroker.PublishAsync(event);   // ✗ Fails - message lost!
```

Or worse:
```csharp
await _messageBroker.PublishAsync(event);   // ✓ Succeeds
await _dbContext.SaveChangesAsync();        // ✗ Fails - ghost message!
```

### Outbox Solution

1. Write event to `OutboxMessages` table in same transaction as business data
2. Background processor reads outbox and publishes to message broker
3. Mark as processed after successful publish

**Guarantees**: At-least-once delivery (idempotent handlers required).

---

## Why Specification Pattern?

### The Problem

Query logic scattered and duplicated:

```csharp
// In OrderRepository
public IEnumerable<Order> GetPendingOrders() => 
    _context.Orders.Where(o => o.Status == OrderStatus.Pending);

// Same logic duplicated elsewhere
public IEnumerable<Order> GetPendingOrdersForCustomer(string customerId) => 
    _context.Orders.Where(o => o.Status == OrderStatus.Pending && o.CustomerId == customerId);
```

### Specification Solution

Encapsulate query logic in composable specifications:

```csharp
var pendingSpec = new OrderByStatusSpecification(OrderStatus.Pending);
var customerSpec = new OrderByCustomerSpecification(customerId);

// Compose specifications
var combinedSpec = pendingSpec.And(customerSpec);
var orders = await _repository.ListAsync(combinedSpec);
```

**Benefits**:
- **Reusable** - Same spec used across repository methods
- **Testable** - Specs can be unit tested in isolation
- **Composable** - Combine with And/Or/Not operators
- **Domain-centric** - Query logic lives near domain

---

## Why Strongly Typed IDs?

### The Problem

Primitive obsession leads to bugs:

```csharp
public void AssignOrderToCustomer(Guid orderId, Guid customerId)
{
    // Easy to swap parameters by mistake
    _service.AssignOrderToCustomer(customerId, orderId); // Compiles, but wrong!
}
```

### Strongly Typed IDs Solution

```csharp
public void AssignOrderToCustomer(OrderId orderId, CustomerId customerId)
{
    // Compiler prevents parameter swapping
    _service.AssignOrderToCustomer(customerId, orderId); // Won't compile!
}
```

**Benefits**:
- **Type safety** - Compiler catches ID mix-ups
- **Self-documenting** - Method signatures are clearer
- **Encapsulation** - Can add validation, generation logic

---

## Why Pipeline Behaviors?

### The Problem

Cross-cutting concerns scattered across handlers:

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<OrderId>>
{
    public async Task<Result<OrderId>> Handle(CreateOrderCommand request, ...)
    {
        _logger.LogInformation("Handling CreateOrderCommand..."); // Logging
        var validation = _validator.Validate(request);            // Validation
        if (!validation.IsValid) return Result.Failure(...);
        
        using var transaction = await _db.BeginTransactionAsync(); // Transaction
        try {
            // Actual business logic finally...
        }
    }
}
```

### Pipeline Behaviors Solution

MediatR pipeline handles cross-cutting concerns:

```
Request → [Logging] → [Validation] → [Transaction] → Handler → Response
```

**Benefits**:
- **Separation of concerns** - Handlers contain only business logic
- **Consistent** - All requests go through same pipeline
- **Configurable** - Add/remove behaviors without touching handlers
- **Testable** - Test handlers without cross-cutting noise

---

## Why Manual Minimal APIs (No Carter)?

### Decision Rationale

We chose manual Minimal API endpoints over Carter library:

**Pros of manual approach**:
- **No external dependency** - One less package to update
- **Full control** - Direct access to ASP.NET Core features
- **Transparency** - No magic, clear endpoint registration
- **Official support** - Microsoft-backed, well-documented

**When to use Carter**: Large projects with many endpoints where convention-based routing provides significant value.

---

## Why InMemoryEventBus (Not RabbitMQ)?

### Decision Rationale

This template includes `InMemoryEventBus` as default:

- **Zero infrastructure** - Works out of the box
- **Development friendly** - No Docker/broker setup needed
- **Demonstrates pattern** - Shows the abstraction correctly

**For production**, implement `IEventBus` with:
- RabbitMQ (self-hosted)
- Azure Service Bus (cloud)
- Amazon SQS (AWS)

The abstraction makes swapping implementations a single class change.

---

## Architecture Tests

The template enforces architecture rules at compile time:

```csharp
[Fact]
public void Domain_Should_Not_Reference_Infrastructure()
{
    var result = Types.InAssembly(DomainAssembly)
        .ShouldNot()
        .HaveDependencyOn("Infrastructure")
        .GetResult();
    
    result.IsSuccessful.Should().BeTrue();
}
```

**Why?** Architecture degrades over time. Automated tests prevent accidental violations.

---

## Summary

| Decision | Why |
|----------|-----|
| Clean Architecture | Dependency inversion, testable domain |
| Vertical Slices | Feature cohesion, low coupling |
| Pragmatic DDD | Apply patterns where they add value |
| CQRS | Optimize reads and writes separately |
| Result Pattern | Explicit error handling, no exception abuse |
| Hybrid Events | Immediate for consistency, Outbox for reliability |
| Specifications | Reusable, composable query logic |
| Strongly Typed IDs | Prevent primitive obsession bugs |
| Pipeline Behaviors | Clean separation of cross-cutting concerns |

---

## License

MIT License - see [LICENSE](LICENSE) file for details.
