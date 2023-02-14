using Lazy.Domain.Primitives;

namespace Lazy.Domain.DomainEvents;

public abstract record DomainEvent(Guid Id) : IDomainEvent;