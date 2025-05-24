using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Users.RegisterUser;

public record RegisterUserCommnad(string Username, string Password) : ICommand<string?>;
