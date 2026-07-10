using Chat.Application.UseCases.Messages.GetMessagesByRoom;

namespace Chat.Application.Abstractions.Data;

public interface IMessageDao
{
    Task<IEnumerable<GetMessagesByRoomResponse>> GetByRoom(Guid roomId, DateTime? before, int take, CancellationToken cancellationToken);
}
