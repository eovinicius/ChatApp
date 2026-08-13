using BuildingBlocks.Api;

using SharedKernel;

namespace ChatApp.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ocorreu uma exceção: {Message}", exception.Message);

            // Se já começamos a escrever, não dá para trocar o corpo: deixa a
            // exceção subir para o servidor abortar a conexão.
            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();

            // Mesmo caminho de construção dos erros dos endpoints — o formato não
            // pode divergir só porque a falha veio de uma exceção.
            await ApiResults.Problem(ToError(exception)).ExecuteAsync(context);
        }
    }

    private static Error ToError(Exception exception) => exception switch
    {
        UnauthorizedAccessException => new Error(
            "Auth.Forbidden",
            "Você não tem permissão para realizar esta ação.",
            ErrorType.Forbidden),

        // Corpo ilegível/JSON malformado: o Minimal API lança isso antes do handler.
        BadHttpRequestException => new Error(
            "Http.MalformedRequest",
            "A requisição não pôde ser lida. Verifique o corpo enviado.",
            ErrorType.Validation),

        ArgumentException argumentException => new Error(
            "Request.InvalidArgument",
            argumentException.Message,
            ErrorType.Validation),

        // A mensagem real fica só no log — nunca no corpo da resposta.
        _ => new Error(
            "Server.Unexpected",
            "Ocorreu um erro inesperado.",
            ErrorType.Failure)
    };
}
