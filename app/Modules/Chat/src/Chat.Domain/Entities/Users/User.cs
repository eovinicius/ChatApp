using Chat.Domain.Events;

using SharedKernel;

namespace Chat.Domain.Entities.Users;

public sealed class User : AggregateRoot
{
    public string Name { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }

    private User() { }

    private User(string name, string username, string password)
        : base(Guid.NewGuid())
    {
        Name = name;
        Username = username;
        Password = password;
    }

    public static Result<User> Create(string name, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            return UserErrors.EmptyName;

        if (string.IsNullOrWhiteSpace(username))
            return UserErrors.EmptyUsername;

        if (string.IsNullOrWhiteSpace(password))
            return UserErrors.EmptyPassword;

        var user = new User(name, username, password);
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Username));
        return Result.Success(user);
    }
}