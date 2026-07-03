using Chat.Application.Abstractions.Messaging;

namespace Chat.Application.UseCases.Users.Login;

public record LoginCommand(string Username, string Password) : ICommand<string>;
