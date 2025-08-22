using MediatR;

using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace ChatApp.Application.Abstractions.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int WARNING_THRESHOLD_MILLISECONDS = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > WARNING_THRESHOLD_MILLISECONDS)
            {
                var requestName = typeof(TRequest).Name;

                var userId = "N/A";
                var userName = "system";

                _logger.LogWarning(
                    "Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) UserID: {UserId} UserName: {UserName} RequestData: {@Request}",
                    requestName,
                    elapsedMilliseconds,
                    userId,
                    userName,
                    request
                );
            }
        }
    }
}