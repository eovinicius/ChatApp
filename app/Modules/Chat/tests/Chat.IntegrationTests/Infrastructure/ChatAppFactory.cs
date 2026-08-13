using Chat.Application.Abstractions.Storage;
using Chat.Infrastructure.Database.EntityFramework;

using Identity.Infrastructure.Database;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NSubstitute;

using Testcontainers.PostgreSql;

namespace Chat.IntegrationTests.Infrastructure;

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

        // Um DbContext por módulo, em schemas separados ("identity" e "chat").
        await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<ChatAppDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _postgres.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            ReplaceDbContext<ChatAppDbContext>(services);
            ReplaceDbContext<IdentityDbContext>(services);

            // Nunca chamar a AWS de verdade nos testes.
            services.RemoveAll<IFileStorageService>();
            services.AddSingleton(FileStorageMock);
        });
    }

    private void ReplaceDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        services.RemoveAll<DbContextOptions<TContext>>();
        services.RemoveAll<TContext>();

        var configDescriptors = services
            .Where(d => d.ServiceType.IsGenericType &&
                        d.ServiceType.GenericTypeArguments.Length == 1 &&
                        d.ServiceType.GenericTypeArguments[0] == typeof(TContext) &&
                        d.ServiceType.Name.Contains("DbContextOptionsConfiguration"))
            .ToList();

        foreach (var descriptor in configDescriptors)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));
    }
}
