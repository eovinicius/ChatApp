using Asp.Versioning;
using Asp.Versioning.Builder;

using Chat.Application;
using Chat.Infrastructure;
using Chat.Infrastructure.RealTime;
using Chat.Presentation.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chat.Presentation;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);

        return services;
    }

    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        app.MapUserEndpoints(versionSet);
        app.MapChatRoomEndpoints(versionSet);
        app.MapMessageEndpoints(versionSet);

        app.MapHub<ChatHub>("/chatHub");

        return app;
    }
}
