namespace ChatApp.Application.Abstractions.Services;

public interface IHashService
{
    string Hash(string password);
    bool Compare(string password, string passwordHash);
}
