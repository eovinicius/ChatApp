using Asp.Versioning;
using Asp.Versioning.Builder;

using Identity.Application;
using Identity.Infrastructure;
using Identity.Presentation.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation;

// Dono da autenticação da aplicação: registra o esquema JWT, o IUserContext e os
// usuários. Os outros módulos só conhecem UserId + Identity.Contracts.
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        app.MapAuthEndpoints(versionSet);
        app.MapUserEndpoints(versionSet);

        return app;
    }
}
