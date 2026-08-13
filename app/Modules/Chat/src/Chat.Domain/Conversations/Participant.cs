using SharedKernel;

namespace Chat.Domain.Conversations;

// Membro de uma conversa. Guarda também os cursores de entrega/leitura, que são a
// base dos recibos: em vez de uma linha por (mensagem × usuário), cada participante
// tem uma marca d'água e o status de cada mensagem é derivado por comparação.
public sealed class Participant : Entity
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public ParticipantRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    // SentAt da última mensagem lida/entregue — não é o instante do ack.
    // É isso que faz "não lidas = mensagens com SentAt > LastReadMessageSentAt" funcionar.
    public DateTime? LastReadMessageSentAt { get; private set; }
    public Guid? LastReadMessageId { get; private set; }
    public DateTime? LastDeliveredMessageSentAt { get; private set; }

    public bool IsMuted { get; private set; }

    public bool IsActive => LeftAt is null;
    public bool IsAdmin => Role is ParticipantRole.Admin or ParticipantRole.Owner;
    public bool IsOwner => Role == ParticipantRole.Owner;

    private Participant() { }

    private Participant(Guid conversationId, Guid userId, ParticipantRole role, DateTime utcNow)
        : base(Guid.NewGuid())
    {
        ConversationId = conversationId;
        UserId = userId;
        Role = role;
        JoinedAt = utcNow;
    }

    internal static Participant Create(Guid conversationId, Guid userId, ParticipantRole role, DateTime utcNow)
        => new(conversationId, userId, role, utcNow);

    internal void ChangeRole(ParticipantRole role) => Role = role;

    internal void Leave(DateTime utcNow) => LeftAt = utcNow;

    internal void Rejoin(DateTime utcNow)
    {
        LeftAt = null;
        JoinedAt = utcNow;
    }

    // Monotônico: um ack fora de ordem (rede, múltiplos dispositivos) não pode
    // retroceder o cursor e ressuscitar mensagens já lidas.
    internal bool MarkRead(Guid messageId, DateTime messageSentAt)
    {
        if (LastReadMessageSentAt is not null && messageSentAt <= LastReadMessageSentAt)
            return false;

        LastReadMessageSentAt = messageSentAt;
        LastReadMessageId = messageId;

        // Ler implica ter recebido.
        if (LastDeliveredMessageSentAt is null || messageSentAt > LastDeliveredMessageSentAt)
            LastDeliveredMessageSentAt = messageSentAt;

        return true;
    }

    internal bool MarkDelivered(DateTime messageSentAt)
    {
        if (LastDeliveredMessageSentAt is not null && messageSentAt <= LastDeliveredMessageSentAt)
            return false;

        LastDeliveredMessageSentAt = messageSentAt;
        return true;
    }

    public bool HasRead(DateTime messageSentAt)
        => LastReadMessageSentAt is not null && LastReadMessageSentAt >= messageSentAt;

    public bool HasReceived(DateTime messageSentAt)
        => LastDeliveredMessageSentAt is not null && LastDeliveredMessageSentAt >= messageSentAt;

    public void Mute() => IsMuted = true;

    public void Unmute() => IsMuted = false;
}
