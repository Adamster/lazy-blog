namespace Lazy.Domain.DomainEvents;

public record UserRegisteredDomainEvent(Guid Id, Guid UserId) : DomainEvent(Id);