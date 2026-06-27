using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

using Dapper;


namespace ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

public class GetMessagesByRoomQueryHandler : IQueryHandler<GetMessagesByRoomQuery, IEnumerable<GetMessagesByRoomResponse>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;

    public GetMessagesByRoomQueryHandler(ISqlConnectionFactory sqlConnectionFactory, IUserContext userContext, IUserRepository userRepository, IChatRoomRepository chatRoomRepository)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
    }

    public async Task<Result<IEnumerable<GetMessagesByRoomResponse>>> Handle(GetMessagesByRoomQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var user = await _userRepository.GetById(currentUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<IEnumerable<GetMessagesByRoomResponse>>(Error.NullValue);
        }

        var chatRoom = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);

        if (chatRoom is null)
        {
            return Result.Failure<IEnumerable<GetMessagesByRoomResponse>>(Error.NullValue);
        }

        if (!chatRoom.IsUserInRoom(user))
        {
            return Result.Failure<IEnumerable<GetMessagesByRoomResponse>>(Error.NullValue);
        }

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
