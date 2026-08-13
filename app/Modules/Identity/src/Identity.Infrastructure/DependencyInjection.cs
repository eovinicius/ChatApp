using System.Security.Claims;
using System.Text;

using BuildingBlocks.Authentication;

using Identity.Application.Abstractions;
using Identity.Contracts;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Authentication;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Database;
using Identity.Infrastructure.Database.Repositories;
using Identity.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    // O hub SignalR fica nesta rota; o handshake WebSocket manda o JWT na query string.
    private const string ChatHubPath = "/chatHub";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddAuthentication(services, configuration);

        services.AddScoped<IHashService, HashService>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<IdentityDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'Database' não configurada. Configure em appsettings.json ou variáveis de ambiente.");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        // Contrato público consumido pelos outros módulos.
        services.AddScoped<UserDirectory>();
        services.AddScoped<IUserDirectory>(sp => sp.GetRequiredService<UserDirectory>());
        services.AddScoped<IUserPresenceSink>(sp => sp.GetRequiredService<UserDirectory>());
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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = ClaimTypes.Role,
            };

            // O cliente SignalR não consegue mandar o header Authorization no handshake
            // WebSocket; sem isto o hub simplesmente não autentica.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    if (!string.IsNullOrEmpty(accessToken) &&
                        context.HttpContext.Request.Path.StartsWithSegments(ChatHubPath))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
    }
}
