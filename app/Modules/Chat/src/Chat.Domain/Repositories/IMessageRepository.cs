using Chat.Domain.Messages;

namespace Chat.Domain.Repositories;

public interface IMessageRepository
{
    Task Add(Message message, CancellationToken cancellationToken = default);

    Task<Message?> GetById(Guid messageId, CancellationToken cancellationToken = default);

    void Update(Message message);
}
