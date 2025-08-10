using ChatApp.Application.Abstractions.Services;

using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.Services;

public class ChatHub : Hub<IChatHub>
{
    public async Task JoinRoom(string roodId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roodId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendMessage(string roomName, string user, string message)
    {
        await Clients.Group(roomName).SendMessageToGroup(roomName, $"{user}: {message}");
    }
}
