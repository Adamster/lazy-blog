using Lazy.DataContracts.Author;

namespace Lazy.Services.Providers;

public interface IJwtProvider
{
     string Generate(AuthorItemDto author);
}