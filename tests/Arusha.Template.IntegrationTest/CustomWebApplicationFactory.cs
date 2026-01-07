namespace Arusha.Template.IntegrationTest;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Uses SQLite in-memory database to support transactions (required by TransactionBehavior).
/// The SQLite connection is kept open for the lifetime of the factory.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection _sqliteConnection;
    private bool _isInitialized;

    public CustomWebApplicationFactory()
    {
        // Create and open SQLite in-memory connection early
        // Important: Connection must stay open for the lifetime of the in-memory database
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        _sqliteConnection.Open();
    }

    public Task InitializeAsync()
    {
        // Initialization happens on first client creation
        return Task.CompletedTask;
    }

    public new HttpClient CreateClient(WebApplicationFactoryClientOptions options = null)
    {
        var client = base.CreateClient(options ?? new WebApplicationFactoryClientOptions());

        // Initialize database schema on first call
        if (!_isInitialized)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            _isInitialized = true;
        }

        return client;
    }

    public new async Task DisposeAsync()
    {
        if (_sqliteConnection != null)
        {
            await _sqliteConnection.DisposeAsync();
        }
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set Test environment - Program.cs will skip Infrastructure and Auth registration
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Add test-specific Infrastructure using SQLite
            services.AddInfrastructureForTesting(_sqliteConnection);

            // Add test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", schemeOptions =>
            {
                schemeOptions.TimeProvider = TimeProvider.System;
            });
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        // Use overridden CreateClient which handles database initialization
        var client = CreateClient(new WebApplicationFactoryClientOptions());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, "test-token");
        return client;
    }
}

internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "integration-user"),
            new Claim(ClaimTypes.Name, "Integration Test User")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
