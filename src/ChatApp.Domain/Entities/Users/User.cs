namespace ChatApp.Domain.Entities.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public User(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

}