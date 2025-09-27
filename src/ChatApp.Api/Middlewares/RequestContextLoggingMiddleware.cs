using CorrelationId.Abstractions;

using Serilog.Context;

namespace ChatApp.Api.Middlewares;

public class RequestContextLoggingMiddleware
{
    private const string HeaderKey = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ICorrelationContextAccessor _correlationAccessor;

    public RequestContextLoggingMiddleware(RequestDelegate next, ICorrelationContextAccessor correlationAccessor)
    {
        _next = next;
        _correlationAccessor = correlationAccessor;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = _correlationAccessor.CorrelationContext?.CorrelationId
            ?? (context.Request.Headers.TryGetValue(HeaderKey, out var cid)
                ? cid.ToString()
                : context.TraceIdentifier);

        context.Items[HeaderKey] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}