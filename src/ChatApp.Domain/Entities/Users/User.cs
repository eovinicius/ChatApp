using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }

    private User() { }

    private User(string name, string username, string password)
    {
        Id = Guid.NewGuid();
        Name = name;
        Username = username;
        Password = password;
    }

    public static Result<User> Create(string name, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(UserErrors.EmptyName);

        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<User>(UserErrors.EmptyUsername);

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<User>(UserErrors.EmptyPassword);

        return Result.Success(new User(name, username, password));
    }
}