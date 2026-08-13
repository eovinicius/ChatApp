using System.ComponentModel.DataAnnotations;

namespace Chat.Presentation.Requests;

public sealed class StartDirectConversationRequest
{
    [Required(ErrorMessage = "O usuário destinatário é obrigatório")]
    public Guid TargetUserId { get; set; }
}

public sealed class CreateGroupConversationRequest
{
    [Required(ErrorMessage = "O nome do grupo é obrigatório")]
    [MinLength(1, ErrorMessage = "O nome do grupo é obrigatório")]
    [MaxLength(100, ErrorMessage = "O nome do grupo deve ter no máximo 100 caracteres")]
    public string Name { get; set; } = default!;

    public IReadOnlyCollection<Guid> MemberIds { get; set; } = [];
}

public sealed class RenameConversationRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Name { get; set; } = default!;
}

public sealed class AddParticipantsRequest
{
    [Required(ErrorMessage = "Informe ao menos um usuário")]
    [MinLength(1, ErrorMessage = "Informe ao menos um usuário")]
    public IReadOnlyCollection<Guid> UserIds { get; set; } = [];
}

public sealed class MarkAsReadRequest
{
    [Required(ErrorMessage = "A última mensagem lida é obrigatória")]
    public Guid LastMessageId { get; set; }
}
