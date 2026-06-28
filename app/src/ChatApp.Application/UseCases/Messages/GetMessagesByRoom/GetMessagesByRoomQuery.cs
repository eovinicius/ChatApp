using ChatApp.Application.Abstractions.Messaging;

namespace ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

public record GetMessagesByRoomQuery(Guid RoomId, DateTime? Before, int Take) : IQuery<IEnumerable<GetMessagesByRoomResponse>> { }
