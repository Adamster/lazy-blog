using MediatR;

namespace Lazy.Domain.Primitives;

public interface IDomainEvent : INotification
{
    public Guid Id { get; init; }
}