using Chat.Infrastructure.Database.EntityFramework;

using ChatApp.Api.Middlewares;

using Identity.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    // Cada módulo tem o seu DbContext (schemas "chat" e "identity"), então o host
    // precisa migrar os dois.
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.Migrate();
        scope.ServiceProvider.GetRequiredService<ChatAppDbContext>().Database.Migrate();
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static void UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
    }

    public static void UseIpAddressLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<IPAddressLoggingMiddleware>();
    }
}
