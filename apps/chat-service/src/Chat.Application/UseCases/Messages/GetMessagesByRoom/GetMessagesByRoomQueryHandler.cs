using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.Messaging;
using Chat.Domain.Abstractions;
using Chat.Domain.Entities.ChatRooms;
using Chat.Domain.Entities.Users;
using Chat.Domain.Repositories;

namespace Chat.Application.UseCases.Messages.GetMessagesByRoom;

public class GetMessagesByRoomQueryHandler : IQueryHandler<GetMessagesByRoomQuery, IEnumerable<GetMessagesByRoomResponse>>
{
    private readonly IMessageDao _messageDao;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;

    public GetMessagesByRoomQueryHandler(IMessageDao messageDao, IUserContext userContext, IUserRepository userRepository, IChatRoomRepository chatRoomRepository)
    {
        _messageDao = messageDao;
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
            return Result.Failure<IEnumerable<GetMessagesByRoomResponse>>(UserErrors.NotFound);
        }

        var chatRoom = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);

        if (chatRoom is null)
        {
            return Result.Failure<IEnumerable<GetMessagesByRoomResponse>>(ChatRoomErrors.NotFound);
        }

        if (!chatRoom.IsUserInRoom(user))
        {
            return Result.Failure<IEnumerable<GetMessagesByRoomResponse>>(ChatRoomErrors.NotMember);
        }

        var messages = await _messageDao.GetByRoom(request.RoomId, request.Before, request.Take, cancellationToken);

        return Result.Success(messages);
    }
}
