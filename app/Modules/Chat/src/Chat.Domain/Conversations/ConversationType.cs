namespace Chat.Domain.Conversations;

public enum ConversationType
{
    // Conversa 1x1. Membros imutáveis, sem nome e sem administradores.
    Direct = 1,

    // Grupo. Nome obrigatório, dono, administradores e entrada/saída de membros.
    Group = 2
}
