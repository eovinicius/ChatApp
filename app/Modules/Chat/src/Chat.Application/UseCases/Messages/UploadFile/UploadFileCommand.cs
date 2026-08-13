using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Messages.UploadFile;

public record UploadFileCommand(
    string FileName,
    string ContentType,
    Stream Content,
    string Extension) : ICommand<UploadFileCommandResponse>;

// StorageKey volta junto: é o que o cliente devolve em SendMessage e o que permite
// apagar o objeto no S3 depois. A URL pré-assinada expira e não serve como chave.
public record UploadFileCommandResponse(
    string FileUrl,
    string StorageKey,
    string FileName,
    long SizeBytes
);
