using Chat.Application.Abstractions.Data;
using Chat.Application.UseCases.Messages.GetMessages;

using Dapper;

namespace Chat.Infrastructure.Database.Repositories;

public class MessageDao : IMessageDao
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public MessageDao(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<MessageListItem>> GetByConversation(
        Guid conversationId,
        DateTime? before,
        int take,
        CancellationToken cancellationToken = default)
    {
        // Paginação keyset com desempate por Id. Os contadores de leitura/entrega
        // saem dos cursores dos participantes — sem tabela mensagem × usuário.
        // StorageKey não é exposto: é detalhe interno do storage.
        const string sql = """
            SELECT
                m."Id"              AS Id,
                m."ConversationId"  AS ConversationId,
                m."SenderId"        AS SenderId,
                m."Content"         AS Content,
                m."ContentType"     AS ContentType,
                m."FileName"        AS FileName,
                m."SizeBytes"       AS SizeBytes,
                m."SentAt"          AS SentAt,
                m."EditedAt"        AS EditedAt,
                (m."DeletedAt" IS NOT NULL) AS IsDeleted,
                (
                    SELECT COUNT(*) FROM chat."Participants" p
                    WHERE p."ConversationId" = m."ConversationId"
                      AND p."UserId" <> m."SenderId"
                      AND p."LeftAt" IS NULL
                      AND p."LastReadMessageSentAt" >= m."SentAt"
                )::int AS ReadByCount,
                (
                    SELECT COUNT(*) FROM chat."Participants" p
                    WHERE p."ConversationId" = m."ConversationId"
                      AND p."UserId" <> m."SenderId"
                      AND p."LeftAt" IS NULL
                      AND p."LastDeliveredMessageSentAt" >= m."SentAt"
                )::int AS DeliveredToCount,
                (
                    SELECT COUNT(*) FROM chat."Participants" p
                    WHERE p."ConversationId" = m."ConversationId"
                      AND p."UserId" <> m."SenderId"
                      AND p."LeftAt" IS NULL
                )::int AS OtherParticipantCount
            FROM chat."Messages" m
            WHERE m."ConversationId" = @ConversationId
              AND (@Before::timestamptz IS NULL OR m."SentAt" < @Before::timestamptz)
            ORDER BY m."SentAt" DESC, m."Id" DESC
            LIMIT @Take
            """;

        using var connection = _sqlConnectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { ConversationId = conversationId, Before = before, Take = take },
            cancellationToken: cancellationToken);

        var items = await connection.QueryAsync<MessageListItem>(command);

        return items.ToList();
    }
}
