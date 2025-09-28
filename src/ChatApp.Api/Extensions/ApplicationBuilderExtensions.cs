using ChatApp.Api.Middlewares;
using ChatApp.Infrastructure.Database.EntityFramework;

using Microsoft.EntityFrameworkCore;
using Serilog;
using CorrelationId.Abstractions;

namespace ChatApp.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        using var dbContext = scope.ServiceProvider.GetRequiredService<ChatAppDbContext>();

        dbContext.Database.Migrate();
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

    public static void UseHttpRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms [cid: {CorrelationId}]";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var accessor = httpContext.RequestServices.GetService<ICorrelationContextAccessor>();
                var correlationId = accessor?.CorrelationContext?.CorrelationId;
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }
            };
        });
    }
}
