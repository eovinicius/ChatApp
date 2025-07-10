using MediatR;

namespace ChatApp.Application.UseCases.Messages.UploadFile;

public record UploadFileCommand(
    string FileName,
    string ContentType,
    Stream Content,
    string Extension) : IRequest<UploadFileCommandResponse>;


public record UploadFileCommandResponse(
    string FileUrl
);
