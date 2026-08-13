using Asp.Versioning;
using Asp.Versioning.Builder;

using Chat.Application;
using Chat.Infrastructure;
using Chat.Infrastructure.RealTime;
using Chat.Presentation.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Presentation.DependencyInjection;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        app.MapConversationEndpoints(versionSet);
        app.MapMessageEndpoints(versionSet);

        app.MapHub<ChatHub>("/chatHub");

        return app;
    }
}
