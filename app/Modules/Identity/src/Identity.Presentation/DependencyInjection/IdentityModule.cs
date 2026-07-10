using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation;

// Scaffold do módulo Identity — estrutura pronta para composição pela API principal.
// Ainda sem regras de negócio: apenas compõe o módulo de forma no-op.
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: registrar dependências do módulo Identity
        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // TODO: mapear endpoints do módulo Identity
        return app;
    }
}
