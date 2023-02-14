using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;