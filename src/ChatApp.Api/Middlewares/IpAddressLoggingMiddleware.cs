using Serilog.Context;

namespace ChatApp.Api.Middlewares;

public class IPAddressLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public IPAddressLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            using (LogContext.PushProperty("IPAddress", ipAddress))
            {
                await _next(context);
            }

            return;
        }

        await _next(context);
    }
}
