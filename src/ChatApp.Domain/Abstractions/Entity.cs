namespace ChatApp.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; init; }
    public readonly List<IDomainEvent> DomainEvents;

    protected Entity(Guid id)
    {
        Id = id;
        DomainEvents = [];
    }

    protected Entity() { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        return DomainEvents.AsReadOnly();
    }

    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
}