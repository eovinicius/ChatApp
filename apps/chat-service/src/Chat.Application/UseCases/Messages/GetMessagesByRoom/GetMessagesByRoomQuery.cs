using Chat.Application.Abstractions.Messaging;

namespace Chat.Application.UseCases.Messages.GetMessagesByRoom;

public record GetMessagesByRoomQuery(Guid RoomId, DateTime? Before, int Take) : IQuery<IEnumerable<GetMessagesByRoomResponse>> { }
