namespace Identity.Application.Abstractions;

public interface IHashService
{
    string Hash(string password);
    bool Compare(string password, string passwordHash);
}
