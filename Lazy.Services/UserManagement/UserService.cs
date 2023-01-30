using Lazy.DataContracts.Author;
using Lazy.Repository;
using Lazy.Services.Authentication;
using Lazy.Services.Exceptions;

namespace Lazy.Services.UserManagement;

public class UserService : IUserService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IJwtProvider _jwtProvider;

    public UserService(IAuthorRepository authorRepository, IJwtProvider jwtProvider)
    {
        _authorRepository = authorRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> Login(AuthorCredentials credentials)
    {
        //verify if user with this email exists
        AuthorItemDto? author = await _authorRepository.GetByEmail(credentials.Email);
        if (author is null)
        {
            throw new EntityNotFoundException($"Author with {credentials.Email} not found");
        }
        
        string token = _jwtProvider.Generate(author);
        return token;
    }
}