namespace ChatApp.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task Upload(Stream stream, string key, string contentType, CancellationToken cancellationToken = default);
    string GeneratePresignedUrl(string key, TimeSpan expiration);
    Task Delete(string key, CancellationToken cancellationToken = default);
}
