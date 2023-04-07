using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Users.CheckIfUserNameIsUnique;

public record CheckIfUserNameIsUnique(string Username) : IQuery<bool>;