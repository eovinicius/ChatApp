using ChatApp.Application.Abstractions.Services;

using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Infrastructure.Services;

public class ChatHub : Hub<IChatHub>
{

}