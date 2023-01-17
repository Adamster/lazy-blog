using Lazy.DataContracts.Author;
using Lazy.Repository;
using Lazy.Repository.Models.Author;
using Lazy.Services.Exceptions;
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

    public async Task<AuthorItemDto> CreateAuthor(CreateAuthorDto author)
    {
        var authorWebUrl = author.Name;
        var authorDto = new AuthorDto(Guid.NewGuid(), author.Name, authorWebUrl);
        AuthorDto newAuthor =  await _authorRepository.CreateAuthor(authorDto);
        return newAuthor.Adapt<AuthorItemDto>();
    }

    public async Task UpdateAuthor(UpdateAuthorDto updatedAuthorDto)
    {
        AuthorDto? existingAuthor = await _authorRepository.GetById(updatedAuthorDto.Id);
        if (existingAuthor == null)
        {
            throw new EntityNotFoundException($"$Author with id: {updatedAuthorDto.Id} not found");
        }

        AuthorDto newAuthor = existingAuthor with { Name = updatedAuthorDto.Name, WebUrl = updatedAuthorDto.WebUrl };
        if (existingAuthor != newAuthor)
        {
            await _authorRepository.UpdateAuthor(newAuthor);
        }

    }

    public async Task<bool> DeleteById(Guid id)
    {
        AuthorDto? existingAuthor = await _authorRepository.GetById(id);
        if (existingAuthor == null)
        {
            throw new EntityNotFoundException($"$Author with id: {id} not found");
        }

        return await _authorRepository.DeleteByAuthor(id);

    }
}