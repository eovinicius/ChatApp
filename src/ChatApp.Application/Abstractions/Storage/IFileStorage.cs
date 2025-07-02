namespace ChatApp.Application.Abstractions.Storage;

public interface IFileStorage
{
    Task<string> Upload(Stream file, string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
