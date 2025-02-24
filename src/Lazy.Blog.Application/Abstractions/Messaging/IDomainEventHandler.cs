using Lazy.Domain.Primitives;
using MediatR;

namespace Lazy.Application.Abstractions.Messaging;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent> 
    where TEvent : IDomainEvent
{
}