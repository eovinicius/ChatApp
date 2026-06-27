using ChatApp.Domain.Abstractions;

namespace ChatApp.Application.UseCases.Messages.UploadFile;

public static class UploadFileErrors
{
    public static readonly Error FileTooLarge = new("UploadFile.FileTooLarge", "O arquivo excede o tamanho máximo permitido de 50 MB.");
    public static readonly Error InvalidContentType = new("UploadFile.InvalidContentType", "Tipo de conteúdo não permitido. Apenas imagens, áudios e vídeos são aceitos.");
    public static readonly Error EmptyFile = new("UploadFile.EmptyFile", "O arquivo não pode estar vazio.");
}
