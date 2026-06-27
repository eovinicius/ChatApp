using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Storage;
using ChatApp.Domain.Abstractions;

namespace ChatApp.Application.UseCases.Messages.UploadFile;

public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, UploadFileCommandResponse>
{
    private readonly IFileStorageService _fileStorage;

    public UploadFileCommandHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    private const long MaxFileSizeInBytes = 50 * 1024 * 1024;

    private static readonly string[] AllowedContentTypePrefixes = ["image/", "audio/", "video/"];

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".mp4", ".mov", ".webm",
        ".mp3", ".ogg", ".wav", ".m4a",
        ".pdf"
    };

    public async Task<Result<UploadFileCommandResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (request.Content.Length == 0)
            return Result.Failure<UploadFileCommandResponse>(UploadFileErrors.EmptyFile);

        if (request.Content.Length > MaxFileSizeInBytes)
            return Result.Failure<UploadFileCommandResponse>(UploadFileErrors.FileTooLarge);

        if (!AllowedContentTypePrefixes.Any(prefix => request.ContentType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<UploadFileCommandResponse>(UploadFileErrors.InvalidContentType);

        if (!AllowedExtensions.Contains(request.Extension))
            return Result.Failure<UploadFileCommandResponse>(UploadFileErrors.InvalidExtension);

        var key = $"messages/{Guid.NewGuid()}{request.Extension}";

        await _fileStorage.Upload(request.Content, key, request.ContentType, cancellationToken);

        var url = _fileStorage.GeneratePresignedUrl(key, TimeSpan.FromMinutes(10));

        return Result.Success(new UploadFileCommandResponse(url));
    }
}
