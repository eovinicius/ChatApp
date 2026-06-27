using ChatApp.Application.UseCases.Messages.GetMessagesByRoom;

namespace ChatApp.Application.Abstractions.Data;

public interface IMessageDao
{
    Task<IEnumerable<GetMessagesByRoomResponse>> GetByRoom(Guid roomId, DateTime? before, int take, CancellationToken cancellationToken);
}
