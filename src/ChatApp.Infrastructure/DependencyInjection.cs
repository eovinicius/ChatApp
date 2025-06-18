using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Repositories;
using ChatApp.Infrastructure.Authentication;
using ChatApp.Infrastructure.Database.EntityFramework.Data;
using ChatApp.Infrastructure.Database.EntityFramework.Repositories;
using ChatApp.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddAuthentication(services);
        AddServicesProviders(services);
        AddRateLimiter(services);
        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        // var connectionString = configuration.GetConnectionString("Database") ??
        //                        throw new ArgumentNullException(nameof(configuration));
        var connectionString = "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres;";

        services.AddDbContext<ChatAppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddServicesProviders(IServiceCollection services)
    {
        services.AddScoped<IHashService, HashService>();
        services.AddSingleton<IChatHub, SignalRChatRoomNotifier>();
        services.AddSignalR();
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("rV0xK+6G8xZJ3m9rTqMev2Yn1w+8WpFlvT5X8NVa1jJU=")),
            ValidateIssuer = false,
            ValidateAudience = false,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        });
    }

    private static void AddRateLimiter(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("default", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(30),
                        QueueLimit = 0,
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });
    }
}