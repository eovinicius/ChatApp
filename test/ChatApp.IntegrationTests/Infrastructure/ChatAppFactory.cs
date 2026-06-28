using ChatApp.Application.Abstractions.Storage;
using ChatApp.Infrastructure.Database.EntityFramework;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NSubstitute;

using Testcontainers.PostgreSql;

namespace ChatApp.IntegrationTests.Infrastructure;

public class ChatAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("chatapp_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public IFileStorageService FileStorageMock { get; } = Substitute.For<IFileStorageService>();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _postgres.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related descriptors to ensure test container is used
            services.RemoveAll<DbContextOptions<ChatAppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<ChatAppDbContext>();

            // Also remove any IDbContextOptionsConfiguration<ChatAppDbContext> that might exist
            var configDescriptors = services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GenericTypeArguments.Length == 1 &&
                            d.ServiceType.GenericTypeArguments[0] == typeof(ChatAppDbContext) &&
                            d.ServiceType.Name.Contains("DbContextOptionsConfiguration"))
                .ToList();
            foreach (var d in configDescriptors)
                services.Remove(d);

            services.AddDbContext<ChatAppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Replace S3 with mock to avoid real AWS calls
            services.RemoveAll<IFileStorageService>();
            services.AddSingleton(FileStorageMock);
        });
    }
}
