namespace ChatApp.Domain.Entities.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }

    public User(string name, string username, string password)
    {
        Id = Guid.NewGuid();
        Name = name;
        Username = username;
        Password = password;
    }

}