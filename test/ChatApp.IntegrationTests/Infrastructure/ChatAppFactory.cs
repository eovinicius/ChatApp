using ChatApp.Application.Abstractions.Storage;
using ChatApp.Infrastructure.Database.EntityFramework;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        builder.ConfigureServices(services =>
        {
            // Replace real DB connection with test container
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChatAppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<ChatAppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            // Replace S3 with mock to avoid real AWS calls
            var storageDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IFileStorageService));
            if (storageDescriptor is not null)
                services.Remove(storageDescriptor);

            services.AddSingleton(FileStorageMock);
        });
    }
}
