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

        try
        {
            _logger.LogInformation("{RequestName} - Executing request", requestName);

            var result = await next(cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("{RequestName} - Request processed successfully", requestName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    _logger.LogError("{RequestName} - Request failed", requestName);
                }
            }

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{RequestName} Request processing failed", requestName);

            throw;
        }
    }
}