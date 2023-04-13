namespace Lazy.Application.Abstractions.Authorization;

public interface ICurrentUserContext
{
    bool IsCurrentUser(Guid  userId);
}