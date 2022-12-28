using Lazy.DataContracts.Author;
using Lazy.Repository;
using Lazy.Repository.Models.Author;
using Mapster;

namespace Lazy.Services.Author;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<IList<AuthorItemDto>> GetAllAuthors()
    {
        IList<AuthorDto> authors = await _authorRepository.GetAll();
        return authors.Adapt<IList<AuthorItemDto>>();
    }

    public async Task<AuthorItemDto?> GetAuthorById(Guid? id)
    {
        if (id == null)
        {
            return null;
        }
        AuthorDto? author = await _authorRepository.GetById(id.GetValueOrDefault());
        return author?.Adapt<AuthorItemDto>();
    }

    public Task<AuthorItemDto> CreateAuthor(CreateAuthorDto author)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAuthor(UpdateAuthorDto adapt)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteById(Guid id)
    {
        throw new NotImplementedException();
    }
}