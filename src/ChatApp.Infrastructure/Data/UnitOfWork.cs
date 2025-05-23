using ChatApp.Application.Abstractions.Data;

namespace ChatApp.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ChatAppDbContext _context;

    public UnitOfWork(ChatAppDbContext context)
    {
        _context = context;
    }

    public Task Commit(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.DetectChanges();
        return _context.SaveChangesAsync(cancellationToken);
    }

}