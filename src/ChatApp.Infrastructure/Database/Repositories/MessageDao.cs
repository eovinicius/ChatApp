using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

using Dapper;

namespace ChatApp.Infrastructure.Database.Repositories;

public class MessageDao : IMessageDao
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public MessageDao(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IEnumerable<GetMessagesByRoomResponse>> GetByRoom(Guid roomId, DateTime? before, int take, CancellationToken cancellationToken)
    {
        var connection = _sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                content AS Content,
                content_type AS ContentType,
                sender_id AS SenderId,
                send_at AS SentAt
            FROM chat_messages
            WHERE room_id = @RoomId
                AND (@Before IS NULL OR send_at < @Before)
            ORDER BY send_at DESC
            LIMIT @Take
            """;

        return await connection.QueryAsync<GetMessagesByRoomResponse>(
            sql,
            new { RoomId = roomId, Take = take, Before = before }
        );
    }
}
