using SharedKernel;

namespace Chat.Application.UseCases.Conversations;

// Erros do Chat sobre usuários de outro módulo — o Chat não conhece UserErrors do Identity.
public static class UserDirectoryErrors
{
    public static readonly Error TargetNotFound = new("Conversation.TargetUserNotFound", "Usuário destinatário não encontrado.", ErrorType.NotFound);
    public static readonly Error MemberNotFound = new("Conversation.MemberNotFound", "Um ou mais usuários informados não existem.", ErrorType.Validation);
}
