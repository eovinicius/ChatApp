using ChatApp.Application.Abstractions.Messaging;

using ChatApp.Domain.Events;

using Microsoft.Extensions.Logging;

namespace ChatApp.Application.UseCases.Users.RegisterUser;

public sealed class UserRegisteredEventHandler : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
        => _logger = logger;

    public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Usuário registrado: {Username} (Id: {UserId})",
            notification.Username, notification.UserId);

        return Task.CompletedTask;
    }
}
