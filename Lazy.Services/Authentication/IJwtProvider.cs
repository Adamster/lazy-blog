using Lazy.DataContracts.Author;

namespace Lazy.Services.Authentication;

public interface IJwtProvider
{
     string Generate(AuthorItemDto author);
}