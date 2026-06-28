using System.Diagnostics;

using ChatApp.Domain.Abstractions;

using MediatR;

using Microsoft.Extensions.Logging;

using Serilog.Context;

namespace ChatApp.Application.Abstractions.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = request.GetType().Name;

        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("{RequestName} - Executing request", requestName);

            var result = await next(cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("{RequestName} - Request processed successfully in {ElapsedMilliseconds}ms", requestName, sw.ElapsedMilliseconds);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    _logger.LogError("{RequestName} - Request failed in {ElapsedMilliseconds}ms", requestName, sw.ElapsedMilliseconds);
                }
            }

            return result;
        }
        catch (Exception exception)
        {
            sw.Stop();
            _logger.LogError(exception, "{RequestName} - Request processing failed after {ElapsedMilliseconds}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}