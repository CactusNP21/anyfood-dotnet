using MediatR;

namespace Domain.Common;

public class BaseEntity
{
    private readonly List<INotification> _domainEvents = [];
    public IReadOnlyList<INotification> DomainEvents => _domainEvents;

    protected void AddDomainEvent(INotification e) => _domainEvents.Add(e);
    public void ClearDomainEvents() => _domainEvents.Clear();

}