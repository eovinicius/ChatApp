using System.Security.Claims;

using ChatApp.Application.Abstractions.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.RealTime;

[Authorize]
public class ChatHub : Hub<IChatHub>
{
    public async Task JoinRoom(string roomId)
    {
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Anônimo";
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{roomId}");
        await Clients.Group($"chat_{roomId}").JoinGroup(roomId, userName);
    }

    public async Task LeaveRoom(string roomId)
    {
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Anônimo";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{roomId}");
        await Clients.Group($"chat_{roomId}").LeftGroup(roomId, userName);
    }

    public async Task SendMessage(string roomId, string message)
    {
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Anônimo";
        await Clients.Group($"chat_{roomId}").SendMessageToGroup(roomId, $"{userName}: {message}");
    }
}
