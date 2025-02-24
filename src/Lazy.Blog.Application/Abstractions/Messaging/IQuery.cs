using Lazy.Domain.Shared;
using MediatR;

namespace Lazy.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}