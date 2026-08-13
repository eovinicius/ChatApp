using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.RealTime;
using Chat.Domain.Repositories;

using Identity.Contracts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Infrastructure.RealTime;

// Hub enxuto de propósito: só presença e "digitando".
// Enviar mensagem é sempre HTTP → handler → banco → notificador. O hub antigo tinha um
// SendMessage que fazia broadcast sem persistir nada.
[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IPresenceTracker _presenceTracker;
    private readonly IConversationDao _conversationDao;
    private readonly IConversationRepository _conversationRepository;
    private readonly IChatNotifier _notifier;
    private readonly IUserPresenceSink _presenceSink;

    public ChatHub(
        IPresenceTracker presenceTracker,
        IConversationDao conversationDao,
        IConversationRepository conversationRepository,
        IChatNotifier notifier,
        IUserPresenceSink presenceSink)
    {
        _presenceTracker = presenceTracker;
        _conversationDao = conversationDao;
        _conversationRepository = conversationRepository;
        _notifier = notifier;
        _presenceSink = presenceSink;
    }

    public override async Task OnConnectedAsync()
    {
        if (!TryGetUserId(out var userId))
        {
            await base.OnConnectedAsync();
            return;
        }

        var isFirstConnection = await _presenceTracker.Connect(userId, Context.ConnectionId);

        // Só avisa na transição offline→online, não a cada aba aberta.
        if (isFirstConnection)
        {
            var contacts = await _conversationDao.GetContactIds(userId, Context.ConnectionAborted);

            if (contacts.Count > 0)
                await _notifier.PresenceChanged(contacts, userId, isOnline: true, lastSeenAt: null);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!TryGetUserId(out var userId))
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var isLastConnection = await _presenceTracker.Disconnect(userId, Context.ConnectionId);

        if (isLastConnection)
        {
            var lastSeenAt = DateTime.UtcNow;

            // O "visto por último" é persistido pelo Identity, dono do usuário.
            await _presenceSink.TouchLastSeen(userId, lastSeenAt);

            var contacts = await _conversationDao.GetContactIds(userId, CancellationToken.None);

            if (contacts.Count > 0)
                await _notifier.PresenceChanged(contacts, userId, isOnline: false, lastSeenAt);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Efêmero: nada é persistido.
    public async Task Typing(Guid conversationId, bool isTyping)
    {
        if (!TryGetUserId(out var userId))
            return;

        var conversation = await _conversationRepository.GetByIdWithParticipants(conversationId, Context.ConnectionAborted);

        if (conversation is null || !conversation.IsActiveParticipant(userId))
            return;

        var others = conversation.ActiveParticipantIds().Where(id => id != userId).ToList();

        if (others.Count > 0)
            await _notifier.TypingChanged(others, conversationId, userId, isTyping);
    }

    private bool TryGetUserId(out Guid userId) => Guid.TryParse(Context.UserIdentifier, out userId);
}
