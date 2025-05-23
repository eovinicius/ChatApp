using ChatApp.Domain.Entities.Users;

namespace ChatApp.Application.Abstractions.Services;

public interface IChatHub
{
    Task JoinGroup(User user, string groupName);
}