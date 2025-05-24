using ChatApp.Application.Abstractions.Services;

namespace ChatApp.Infrastructure.Services;

public class HashService : IHashService
{
    public string Hash(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return passwordHash;
    }
    public bool Compare(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

}
