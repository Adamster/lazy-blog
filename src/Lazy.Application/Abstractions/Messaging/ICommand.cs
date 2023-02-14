using Lazy.Domain.Shared;
using MediatR;

namespace Lazy.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}