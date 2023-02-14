using Lazy.Domain.Shared;
using MediatR;

namespace Lazy.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> 
    : IRequestHandler<TQuery, Result<TResponse>> 
    where TQuery : IQuery<TResponse>
{
}