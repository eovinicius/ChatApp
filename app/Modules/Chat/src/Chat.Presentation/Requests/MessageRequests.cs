using System.ComponentModel.DataAnnotations;

namespace Chat.Presentation.Requests;

public sealed class SendMessageRequest
{
    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    public string Content { get; set; } = default!;

    [Required(ErrorMessage = "O tipo de conteúdo é obrigatório")]
    public string ContentType { get; set; } = "text";

    // Obrigatórios para mídia: devolvidos pelo endpoint de upload.
    public string? StorageKey { get; set; }
    public string? FileName { get; set; }
    public long? SizeBytes { get; set; }
}

public sealed class EditMessageRequest
{
    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    public string Content { get; set; } = default!;
}
