using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Repositories;
using ChatApp.Infrastructure.Authentication;
using ChatApp.Infrastructure.Data;
using ChatApp.Infrastructure.Repositories;
using ChatApp.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<IChatHub, ChatHub>();
        services.AddSignalR();
    }

    private static void AddAuthentication(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IHashService, HashService>();
    }
}