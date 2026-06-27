using System.Data;

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
                "Content" AS Content,
                "ContentType" AS ContentType,
                "SenderId" AS SenderId,
                "SentAt" AS SentAt
            FROM "Messages"
            WHERE "ChatRoomId" = @RoomId
                AND (@Before IS NULL OR "SentAt" < @Before)
            ORDER BY "SentAt" DESC
            LIMIT @Take
            """;

        var parameters = new DynamicParameters();
        parameters.Add("RoomId", roomId);
        parameters.Add("Take", take);
        parameters.Add("Before", before, DbType.DateTimeOffset);

        return await connection.QueryAsync<GetMessagesByRoomResponse>(sql, parameters);
    }
}
