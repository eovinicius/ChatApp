using Amazon;
using Amazon.Runtime;
using Amazon.S3;

using BuildingBlocks.Clock;

using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.RealTime;
using Chat.Application.Abstractions.Storage;
using Chat.Domain.Repositories;
using Chat.Infrastructure.Database;
using Chat.Infrastructure.Database.EntityFramework;
using Chat.Infrastructure.Database.EntityFramework.Data;
using Chat.Infrastructure.Database.EntityFramework.Repositories;
using Chat.Infrastructure.Database.Repositories;
using Chat.Infrastructure.RealTime;
using Chat.Infrastructure.Services;
using Chat.Infrastructure.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddRealTime(services);
        AddStorage(services, configuration);

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ChatAppDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'Database' não configurada. Configure em appsettings.json ou variáveis de ambiente.");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IConversationDao, ConversationDao>();
        services.AddScoped<IMessageDao, MessageDao>();
    }

    private static void AddRealTime(IServiceCollection services)
    {
        services.AddSignalR();

        services.AddSingleton<IChatNotifier, SignalRChatNotifier>();

        // Singleton porque o estado de conexões é do processo inteiro.
        services.AddSingleton<IPresenceTracker, InMemoryPresenceTracker>();
    }

    private static void AddStorage(IServiceCollection services, IConfiguration configuration)
    {
        var s3Section = configuration.GetRequiredSection("AwsSettings:S3");
        services.Configure<AmazonS3Settings>(s3Section);

        var s3Settings = s3Section.Get<AmazonS3Settings>() ?? new AmazonS3Settings();
        if (!string.IsNullOrEmpty(s3Settings.Region))
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region);

            if (!string.IsNullOrEmpty(s3Settings.AccessKey) && !string.IsNullOrEmpty(s3Settings.SecretKey))
            {
                var creds = new BasicAWSCredentials(s3Settings.AccessKey, s3Settings.SecretKey);
                services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(creds, regionEndpoint));
            }
            else
            {
                services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(regionEndpoint));
            }
        }

        services.AddScoped<IFileStorageService, S3FileStorageService>();
    }
}
