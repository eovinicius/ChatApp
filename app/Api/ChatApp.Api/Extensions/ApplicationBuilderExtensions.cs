using BuildingBlocks.Api;

using Chat.Infrastructure.Database.EntityFramework;

using ChatApp.Api.Middlewares;

using Identity.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using SharedKernel;

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

    // 401, 403, 404 de rota, 405, 415 e 429 são produzidos pelo pipeline, não
    // pelos endpoints — sem isto voltariam com corpo vazio e o cliente precisaria
    // de um caminho especial só para eles.
    public static void UseCustomStatusCodeHandler(this IApplicationBuilder app)
    {
        app.UseStatusCodePages(async context =>
        {
            var http = context.HttpContext;

            // O handshake do SignalR e o Swagger têm contratos próprios.
            if (http.Request.Path.StartsWithSegments("/chatHub") ||
                http.Request.Path.StartsWithSegments("/swagger"))
            {
                return;
            }

            var statusCode = http.Response.StatusCode;

            await ApiResults.Problem(ToError(statusCode), statusCode).ExecuteAsync(http);
        });
    }

    private static Error ToError(int statusCode) => statusCode switch
    {
        StatusCodes.Status401Unauthorized => new Error(
            "Auth.Unauthorized", "Autenticação necessária.", ErrorType.Unauthorized),

        StatusCodes.Status403Forbidden => new Error(
            "Auth.Forbidden", "Acesso negado.", ErrorType.Forbidden),

        StatusCodes.Status404NotFound => new Error(
            "Http.NotFound", "Recurso não encontrado.", ErrorType.NotFound),

        StatusCodes.Status405MethodNotAllowed => new Error(
            "Http.MethodNotAllowed", "Método não permitido para este recurso.", ErrorType.Failure),

        StatusCodes.Status415UnsupportedMediaType => new Error(
            "Http.UnsupportedMediaType", "Formato de conteúdo não suportado.", ErrorType.Failure),

        StatusCodes.Status429TooManyRequests => new Error(
            "Http.TooManyRequests", "Muitas requisições. Tente novamente em instantes.", ErrorType.Failure),

        _ => new Error(
            "Http.RequestFailed", "A requisição não pôde ser concluída.", ErrorType.Failure)
    };

    public static void UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
    }

    public static void UseIpAddressLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<IPAddressLoggingMiddleware>();
    }
}
