using SharedKernel;

namespace Chat.Domain.Conversations;

public static class ConversationErrors
{
    public static readonly Error NotFound = new("Conversation.NotFound", "Conversa não encontrada.", ErrorType.NotFound);
    public static readonly Error NotParticipant = new("Conversation.NotParticipant", "Você não participa desta conversa.", ErrorType.Forbidden);
    public static readonly Error ParticipantNotFound = new("Conversation.ParticipantNotFound", "Participante não encontrado na conversa.", ErrorType.NotFound);
    public static readonly Error AlreadyParticipant = new("Conversation.AlreadyParticipant", "O usuário já participa desta conversa.", ErrorType.Conflict);
    public static readonly Error Full = new("Conversation.Full", "A conversa atingiu o limite de participantes.", ErrorType.Conflict);

    public static readonly Error DirectIsImmutable = new("Conversation.DirectIsImmutable", "Conversas 1x1 não permitem alterar participantes nem nome.", ErrorType.Conflict);
    public static readonly Error DirectWithSelf = new("Conversation.DirectWithSelf", "Não é possível iniciar uma conversa consigo mesmo.", ErrorType.Validation);

    public static readonly Error EmptyGroupName = new("Conversation.EmptyGroupName", "O nome do grupo não pode ser vazio.", ErrorType.Validation);
    public static readonly Error RequiresAdmin = new("Conversation.RequiresAdmin", "Apenas administradores podem executar esta ação.", ErrorType.Forbidden);
    public static readonly Error RequiresOwner = new("Conversation.RequiresOwner", "Apenas o dono do grupo pode executar esta ação.", ErrorType.Forbidden);
    public static readonly Error OwnerMustTransferFirst = new("Conversation.OwnerMustTransferFirst", "Transfira a propriedade do grupo antes de sair.", ErrorType.Conflict);
    public static readonly Error CannotRemoveOwner = new("Conversation.CannotRemoveOwner", "O dono do grupo não pode ser removido.", ErrorType.Forbidden);
}
