using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Abstractions;

using Dapper;


namespace ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

public class GetMessagesByRoomQueryHandler : IQueryHandler<GetMessagesByRoomQuery, IEnumerable<GetMessagesByRoomResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetMessagesByRoomQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IUserContext userContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<IEnumerable<GetMessagesByRoomResponse>>> Handle(GetMessagesByRoomQuery request, CancellationToken cancellationToken)
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

        var messages = await connection.QueryAsync<GetMessagesByRoomResponse>(
            sql,
            new { RoomId = request.RoomId, Take = request.Take, Before = request.Before }
        );

        return Result.Success(messages);
    }
}
