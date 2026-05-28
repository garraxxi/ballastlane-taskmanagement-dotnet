using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration testing the API.
/// Each test class gets its own isolated LiteDB instance (using a unique temp file).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;

    public CustomWebApplicationFactory()
    {
        // Unique database file per factory instance for test isolation
        _dbPath = Path.Combine(Path.GetTempPath(), $"TaskManagement_Test_{Guid.NewGuid()}.db");
    }

    public string DatabasePath => _dbPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production LiteDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(LiteDbContext));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register a test-specific LiteDbContext using our unique temp file
            services.AddSingleton<LiteDbContext>(_ => new LiteDbContext(_dbPath));

            // Ensure the database is seeded for tests
            // The existing seeding logic in Program.cs will run on first use
        });

        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up the temporary test database file
            if (File.Exists(_dbPath))
            {
                try { File.Delete(_dbPath); } catch { /* ignore cleanup errors */ }
            }
        }

        base.Dispose(disposing);
    }
}
