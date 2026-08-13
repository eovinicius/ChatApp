using SharedKernel;

namespace Chat.Domain.Entities.Messages;

public static class ChatMessageErrors
{
    public static readonly Error NotFound = new("ChatMessage.NotFound", "Mensagem não encontrada.", ErrorType.NotFound);
    public static readonly Error Unauthorized = new("ChatMessage.Unauthorized", "Você não tem permissão para realizar esta ação nesta mensagem.", ErrorType.Forbidden);
    public static readonly Error EditWindowExpired = new("ChatMessage.EditWindowExpired", "O prazo de 1 hora para editar a mensagem expirou.", ErrorType.Conflict);
    public static readonly Error NotTextMessage = new("ChatMessage.NotTextMessage", "Apenas mensagens de texto podem ser editadas.", ErrorType.Conflict);
    public static readonly Error EmptyContent = new("ChatMessage.EmptyContent", "O conteúdo da mensagem não pode ser vazio.", ErrorType.Validation);
}
