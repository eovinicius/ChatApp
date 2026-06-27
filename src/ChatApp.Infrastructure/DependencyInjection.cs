using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.Abstractions.Storage;
using ChatApp.Domain.Repositories;
using ChatApp.Infrastructure.Authentication;
using ChatApp.Infrastructure.Database.EntityFramework;
using ChatApp.Infrastructure.Database.EntityFramework.Data;
using ChatApp.Infrastructure.Database.EntityFramework.Repositories;
using ChatApp.Infrastructure.Services;
using ChatApp.Infrastructure.Storage;
using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using ChatApp.Infrastructure.Database;
using ChatApp.Infrastructure.Database.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ChatApp.Application.Abstractions.Clock;

namespace ChatApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddAuthentication(services, configuration);
        AddServicesProviders(services, configuration);
        AddRateLimiter(services);
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

        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IMessageDao, MessageDao>();
    }

    private static void AddServicesProviders(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IHashService, HashService>();
        services.AddSingleton<IChatHub, SignalRChatRoomNotifier>();
        services.AddSignalR();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

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

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        var jwtSecretKey = configuration.GetSection("JwtSettings:SecretKey").Value
            ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurado no appsettings.json");

        var jwtIssuer = configuration.GetSection("JwtSettings:Issuer").Value
            ?? throw new InvalidOperationException("JwtSettings:Issuer não configurado.");
        var jwtAudience = configuration.GetSection("JwtSettings:Audience").Value
            ?? throw new InvalidOperationException("JwtSettings:Audience não configurado.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        });
    }

    private static void AddRateLimiter(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));

            options.AddPolicy("chat", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });
    }
}