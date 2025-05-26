
using ChatApp.Application.Abstractions.Services;

using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.Services;

public class SignalRChatRoomNotifier : IChatHub
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRChatRoomNotifier(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task JoinGroup(string roomId, string user, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomId)
            .SendAsync("UserJoined", user);
    }

    public async Task LeftGroup(string roomId, string user, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomId)
            .SendAsync("UserLeft", user);
    }

    public async Task SendMessage(string roomId, string message, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomId).SendAsync("ReceiveMessage", roomId, message);
    }
}
