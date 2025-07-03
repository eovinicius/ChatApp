namespace ChatApp.Application.Abstractions.Services;

public interface IChatHub
{
    Task JoinGroup(string roomId, string user, CancellationToken cancellationToken = default);
    Task LeftGroup(string roomId, string user, CancellationToken cancellationToken = default);
    Task SendMessageToGroup(string roomId, string message, CancellationToken cancellationToken = default);
}