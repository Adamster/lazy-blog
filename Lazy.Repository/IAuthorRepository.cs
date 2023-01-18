using Lazy.DataContracts.Author;
using Lazy.Domain;

namespace Lazy.Repository;

public interface IAuthorRepository : IRepository<Author>
{
    Task<IList<AuthorItemDto>> GetAll();
    Task<AuthorItemDto?> GetById(Guid id);
    Task<AuthorItemDto> CreateAuthor(AuthorItemDto AuthorItemDto);
    Task UpdateAuthor(AuthorItemDto updatedAuthor);
    Task<bool> DeleteByAuthor(Guid id);
}