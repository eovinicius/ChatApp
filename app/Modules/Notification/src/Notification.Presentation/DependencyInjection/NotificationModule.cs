using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notification.Presentation;

// Scaffold do módulo Notification — estrutura pronta para composição pela API principal.
// Ainda sem regras de negócio: apenas compõe o módulo de forma no-op.
public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: registrar dependências do módulo Notification
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: mapear endpoints do módulo Notification
        return app;
    }
}
