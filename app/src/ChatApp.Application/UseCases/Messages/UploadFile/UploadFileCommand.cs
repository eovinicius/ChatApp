using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.UploadFile;

public record UploadFileCommand(
    string FileName,
    string ContentType,
    Stream Content,
    string Extension) : ICommand<UploadFileCommandResponse>;


public record UploadFileCommandResponse(
    string FileUrl
);
