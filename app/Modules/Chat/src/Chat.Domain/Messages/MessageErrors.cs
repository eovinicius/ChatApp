using SharedKernel;

namespace Chat.Domain.Messages;

public static class MessageErrors
{
    public static readonly Error NotFound = new("Message.NotFound", "Mensagem não encontrada.", ErrorType.NotFound);
    public static readonly Error Unauthorized = new("Message.Unauthorized", "Você não pode alterar esta mensagem.", ErrorType.Forbidden);
    public static readonly Error EditWindowExpired = new("Message.EditWindowExpired", "O prazo para editar esta mensagem expirou.", ErrorType.Conflict);
    public static readonly Error DeleteWindowExpired = new("Message.DeleteWindowExpired", "O prazo para apagar esta mensagem expirou.", ErrorType.Conflict);
    public static readonly Error NotTextMessage = new("Message.NotTextMessage", "Apenas mensagens de texto podem ser editadas.", ErrorType.Conflict);
    public static readonly Error EmptyContent = new("Message.EmptyContent", "O conteúdo da mensagem não pode ser vazio.", ErrorType.Validation);
    public static readonly Error AlreadyDeleted = new("Message.AlreadyDeleted", "Esta mensagem já foi apagada.", ErrorType.Conflict);
    public static readonly Error InvalidContentType = new("Message.InvalidContentType", "Tipo de conteúdo não suportado.", ErrorType.Validation);
    public static readonly Error MissingStorageKey = new("Message.MissingStorageKey", "Mensagens de mídia exigem a chave do arquivo no storage.", ErrorType.Validation);
    public static readonly Error WrongConversation = new("Message.WrongConversation", "A mensagem não pertence a esta conversa.", ErrorType.Validation);
}
