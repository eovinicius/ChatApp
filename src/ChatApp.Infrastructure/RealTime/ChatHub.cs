using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Entities.Users;

using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.RealTime;

public class ChatHub : Hub<IChatHub>
{
    public async Task JoinRoom(User user, string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{roomId}");
        await Clients.Group($"chat_{roomId}").JoinGroup(roomId, user.Name);
    }
    public async Task LeaveRoom(User user, string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{roomId}");
        await Clients.Group($"chat_{roomId}").LeftGroup(roomId, user.Name);
    }

    public async Task SendMessage(string roomId, User user, string message)
    {
        await Clients.Group($"chat_{roomId}").SendMessageToGroup(roomId, $"{user}: {message}");
    }
}
