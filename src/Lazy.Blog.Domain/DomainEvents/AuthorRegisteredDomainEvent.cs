namespace Lazy.Domain.DomainEvents;

public sealed record AuthorRegisteredDomainEvent(Guid Id, Guid AuthorId) : DomainEvent(Id);