using Microsoft.AspNetCore.Mvc;

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

            var exceptionDetails = GetExceptionDetails(exception);

            var problemDetails = new ProblemDetails
            {
                Status = exceptionDetails.Status,
                Type = exceptionDetails.Type,
                Title = exceptionDetails.Title,
                Detail = exceptionDetails.Detail,
            };

            context.Response.StatusCode = exceptionDetails.Status;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => new ExceptionDetails(
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "Acesso negado",
                "Você não tem permissão para realizar esta ação",
                null),

            ArgumentException argEx => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                "Argumento inválido",
                argEx.Message,
                null),

            _ => new ExceptionDetails(
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "Erro interno do servidor",
                "Ocorreu um erro inesperado",
                null)
        };
    }

    internal record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors);
}