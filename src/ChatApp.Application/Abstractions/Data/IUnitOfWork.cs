namespace ChatApp.Application.Abstractions.Data;

public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken = default);
}