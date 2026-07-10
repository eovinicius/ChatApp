using BuildingBlocks.Behaviors;

using Microsoft.Extensions.DependencyInjection;

namespace Chat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        return services;
    }
}