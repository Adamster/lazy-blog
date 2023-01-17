using Lazy.DataContracts.Author;

namespace Lazy.Services.Author;

public interface IAuthorService
{
    Task<IList<AuthorItemDto>> GetAllAuthors();
    Task<AuthorItemDto?> GetAuthorById(Guid? id);
    Task<AuthorItemDto> CreateAuthor(CreateAuthorDto author);
    Task UpdateAuthor(UpdateAuthorDto updatedAuthor);
    Task<bool> DeleteById(Guid id);
}