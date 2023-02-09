using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Primitives;
using Lazy.Persistence;
using MediatR;

namespace Lazy.Infrastructure.Idempotence;

public class IdempotentDomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
where TDomainEvent : IDomainEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated;
    private readonly LazyBlogDbContext _dbContext;

    public IdempotentDomainEventHandler(
        INotificationHandler<TDomainEvent> decorated,
        LazyBlogDbContext dbContext)
    {
        _decorated = decorated;
        _dbContext = dbContext;
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        string consumer = _decorated.GetType().Name;
        
        //if(await _dbContext.Set<OutBo>())
    }
}