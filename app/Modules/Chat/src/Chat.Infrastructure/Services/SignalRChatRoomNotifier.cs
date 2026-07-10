using Chat.Application.Abstractions.Services;
using Chat.Infrastructure.RealTime;

using Microsoft.AspNetCore.SignalR;

namespace Chat.Infrastructure.Services;

public class SignalRChatRoomNotifier : IChatHub
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRChatRoomNotifier(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task JoinGroup(string roomId, string user, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group($"chat_{roomId}")
            .SendAsync("UserJoined", user, cancellationToken: cancellationToken);
    }

    public async Task LeftGroup(string roomId, string user, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group($"chat_{roomId}")
            .SendAsync("UserLeft", user, cancellationToken: cancellationToken);
    }

    public async Task SendMessageToGroup(string roomId, string message, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group($"chat_{roomId}").SendAsync("ReceiveMessage", roomId, message, cancellationToken: cancellationToken);
    }
}
