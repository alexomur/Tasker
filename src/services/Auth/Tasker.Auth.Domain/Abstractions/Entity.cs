namespace Tasker.Auth.Domain.Abstractions;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    
    protected void ClearDomainEvents() => _domainEvents.Clear();
}