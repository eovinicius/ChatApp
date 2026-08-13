using Identity.Application.Abstractions;

using MediatR;

using SharedKernel;

namespace Identity.Infrastructure.Database;

public class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _context;
    private readonly IPublisher _publisher;

    public IdentityUnitOfWork(IdentityDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.DetectChanges();
        await _context.SaveChangesAsync(cancellationToken);

        var events = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                var evts = e.GetDomainEvents();
                e.ClearDomainEvents();
                return evts;
            })
            .ToList();

        foreach (var domainEvent in events)
            await _publisher.Publish(domainEvent, cancellationToken);
    }
}
