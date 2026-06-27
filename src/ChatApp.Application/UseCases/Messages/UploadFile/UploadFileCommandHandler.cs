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

    public async Task<Result<UploadFileCommandResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var key = $"messages/{Guid.NewGuid()}{request.Extension}";

        await _fileStorage.Upload(request.Content, key, request.ContentType, cancellationToken);

        var url = _fileStorage.GeneratePresignedUrl(key, TimeSpan.FromMinutes(10));

        return Result.Success(new UploadFileCommandResponse(url));
    }
}
