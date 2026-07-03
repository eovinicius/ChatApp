namespace Chat.Application.Abstractions.Data;

public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken = default);
}