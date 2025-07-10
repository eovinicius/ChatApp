using ChatApp.Application.Abstractions.Storage;

using MediatR;

namespace ChatApp.Application.UseCases.Messages.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadFileCommandResponse>
{
    private readonly IFileStorageService _fileStorage;

    public UploadFileCommandHandler(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<UploadFileCommandResponse> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var key = $"messages/{Guid.NewGuid()}{request.Extension}";

        await _fileStorage.Upload(request.Content, key, request.ContentType, cancellationToken);

        var url = _fileStorage.GeneratePresignedUrl(key, TimeSpan.FromMinutes(10));

        return new UploadFileCommandResponse(url);
    }
}
