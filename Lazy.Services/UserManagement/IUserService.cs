using Lazy.DataContracts.Author;

namespace Lazy.Services.UserManagement;

public interface IUserService
{
    public Task<string> Login(AuthorCredentials credentials);
}