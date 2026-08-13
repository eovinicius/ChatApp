using Chat.Application.Abstractions.Data;
using Chat.Application.UseCases.Conversations.GetMyConversations;

using Dapper;

namespace Chat.Infrastructure.Database.Repositories;

public class ConversationDao : IConversationDao
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ConversationDao(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<ConversationListItem>> GetForUser(
        Guid userId,
        DateTime? before,
        int take,
        CancellationToken cancellationToken = default)
    {
        // Um round-trip só: conversa + o outro participante (se 1x1) + última mensagem
        // + não-lidas derivadas do cursor de leitura do próprio usuário.
        const string sql = """
            SELECT
                c."Id"                  AS Id,
                c."Type"                AS Type,
                c."Name"                AS GroupName,
                c."AvatarUrl"           AS GroupAvatarUrl,
                c."LastActivityAt"      AS LastActivityAt,
                other."UserId"          AS OtherUserId,
                lm."Id"                 AS LastMessageId,
                lm."Content"            AS LastMessagePreview,
                lm."ContentType"        AS LastMessageContentType,
                lm."SenderId"           AS LastMessageSenderId,
                lm."SentAt"             AS LastMessageSentAt,
                (lm."DeletedAt" IS NOT NULL) AS LastMessageDeleted,
                COALESCE(unread.cnt, 0)::int AS UnreadCount
            FROM chat."Conversations" c
            JOIN chat."Participants" me
                ON me."ConversationId" = c."Id"
               AND me."UserId" = @UserId
               AND me."LeftAt" IS NULL
            LEFT JOIN LATERAL (
                SELECT p."UserId"
                FROM chat."Participants" p
                WHERE p."ConversationId" = c."Id"
                  AND p."UserId" <> @UserId
                  AND c."Type" = 1
                LIMIT 1
            ) other ON TRUE
            LEFT JOIN LATERAL (
                SELECT m."Id", m."Content", m."ContentType", m."SenderId", m."SentAt", m."DeletedAt"
                FROM chat."Messages" m
                WHERE m."ConversationId" = c."Id"
                ORDER BY m."SentAt" DESC, m."Id" DESC
                LIMIT 1
            ) lm ON TRUE
            LEFT JOIN LATERAL (
                SELECT COUNT(*) AS cnt
                FROM chat."Messages" m2
                WHERE m2."ConversationId" = c."Id"
                  AND m2."SenderId" <> @UserId
                  AND m2."DeletedAt" IS NULL
                  AND (me."LastReadMessageSentAt" IS NULL OR m2."SentAt" > me."LastReadMessageSentAt")
            ) unread ON TRUE
            WHERE (@Before::timestamptz IS NULL OR c."LastActivityAt" < @Before::timestamptz)
            ORDER BY c."LastActivityAt" DESC
            LIMIT @Take
            """;

        using var connection = _sqlConnectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { UserId = userId, Before = before, Take = take },
            cancellationToken: cancellationToken);

        var items = await connection.QueryAsync<ConversationListItem>(command);

        return items.ToList();
    }

    public async Task<IReadOnlyList<Guid>> GetContactIds(Guid userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT other."UserId"
            FROM chat."Participants" me
            JOIN chat."Participants" other
                ON other."ConversationId" = me."ConversationId"
               AND other."UserId" <> me."UserId"
               AND other."LeftAt" IS NULL
            WHERE me."UserId" = @UserId
              AND me."LeftAt" IS NULL
            """;

        using var connection = _sqlConnectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken);

        var ids = await connection.QueryAsync<Guid>(command);

        return ids.ToList();
    }
}
