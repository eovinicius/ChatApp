using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Entities.Users;

using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.Services;

public class ChatHub : Hub<IChatHub>, IChatHub
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHub(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public Task JoinGroup(User user, string groupName)
    {
        throw new NotImplementedException();
    }

    public Task SendMessageAsync(string user, string message)
    {
        return _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}