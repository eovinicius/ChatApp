using Serilog.Context;

namespace ChatApp.Api.Middlewares;

public class RequestContextLoggingMiddleware
{
    private const string HeaderKey = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderKey, out var cid)
            ? cid.ToString()
            : Guid.NewGuid().ToString();

        context.Items[HeaderKey] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}