using ChatApp.Application.Abstractions.Services;

namespace ChatApp.Infrastructure.Services;

public class HashService : IHashService
{
    private const int SaltSize = 16;
    public string Hash(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(SaltSize);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return passwordHash;
    }
    public bool Compare(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}