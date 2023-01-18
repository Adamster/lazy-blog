using Lazy.DataContracts.Author;
using Lazy.Infrastructure;
using Lazy.Repository;
using Lazy.Services.Exceptions;
using Lazy.Services.Extensions;

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
        IList<AuthorItemDto> authors = await _authorRepository.GetAll();
        return authors;
    }

    public async Task<AuthorItemDto?> GetAuthorById(Guid? id)
    {
        if (id == null)
        {
            return null;
        }
        return await _authorRepository.GetById(id.GetValueOrDefault());
    }

    public async Task<AuthorItemDto> CreateAuthor(CreateAuthorDto author)
    {
        var urlName = author.Name.Slugify();
        var newAuthor = new AuthorItemDto(Guid.Empty, author.Name, urlName);
        return await _authorRepository.CreateAuthor(newAuthor);

    }

    public async Task UpdateAuthor(UpdateAuthorDto updatedAuthorDto)
    {
        AuthorItemDto? existingAuthor = await _authorRepository.GetById(updatedAuthorDto.Id);
        if (existingAuthor == null)
        {
            throw new EntityNotFoundException($"$Author with id: {updatedAuthorDto.Id} not found");
        }

        AuthorItemDto newAuthor = existingAuthor with { Name = updatedAuthorDto.Name, WebUrl = updatedAuthorDto.WebUrl };
        if (existingAuthor != newAuthor)
        {
            await _authorRepository.UpdateAuthor(newAuthor);
        }

    }

    public async Task<bool> DeleteById(Guid id)
    {
        //TODO: remove as soon as authentication implemented
        if (id == Constants.SystemAuthor.SystemAuthorId)
        {
            return true;
        }
        AuthorItemDto? existingAuthor = await _authorRepository.GetById(id);
        if (existingAuthor == null)
        {
            throw new EntityNotFoundException($"$Author with id: {id} not found");
        }

        return await _authorRepository.DeleteByAuthor(id);

    }
}