using ChatApp.Domain.Entities.Users;

namespace ChatApp.Application.Abstractions.Services;

public interface IChatHub
{
    Task JoinGroup(string roomId, string user, CancellationToken cancellationToken = default);
    Task LeftGroup(string roomId, string user, CancellationToken cancellationToken = default);
    Task SendMessage(string roomId, string message, CancellationToken cancellationToken = default);
}