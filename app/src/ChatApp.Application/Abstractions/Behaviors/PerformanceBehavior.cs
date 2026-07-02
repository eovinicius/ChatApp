using ChatApp.Application.Abstractions.Clock;

using MediatR;

using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Abstractions.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    readonly IDateTimeProvider _dateTimeProvider;
    private const int WARNING_THRESHOLD_MILLISECONDS = 3000;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var startedAt = _dateTimeProvider.UtcNow;

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            var elapsedMilliseconds = (_dateTimeProvider.UtcNow - startedAt).TotalMilliseconds;

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