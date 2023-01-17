using Lazy.Domain;
using Lazy.Infrastructure;
using Lazy.Repository.Models.Author;
using Mapster;

namespace Lazy.Repository;

public interface IAuthorRepository : IRepository<Author>
{
    Task<IList<AuthorDto>> GetAll();
    Task<AuthorDto?> GetById(Guid id);
    Task<AuthorDto> CreateAuthor(AuthorDto authorDto);
    Task UpdateAuthor(AuthorDto updatedAuthor);
    Task<bool> DeleteByAuthor(Guid id);
}

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(LazyBlogDbContext lazyBlogDbContext) : base(lazyBlogDbContext)
    {
    }

    public async Task<IList<AuthorDto>> GetAll()
    {
        var items = await GetItems();
        return items.Adapt<IList<AuthorDto>>();
    }

    public async Task<AuthorDto?> GetById(Guid id)
    {
        var result = await GetItemById(id);
        return result?.Adapt<AuthorDto>();
    }

    public async Task<AuthorDto> CreateAuthor(AuthorDto authorDto)
    {
        var author = new Author(authorDto.Name, authorDto.WebUrl);

        await SaveOrUpdate(author, CancellationToken.None);
        return authorDto;
    }

    public async Task UpdateAuthor(AuthorDto updatedAuthor)
    {
        var author = await GetItemById(updatedAuthor.Id);
        if (author == null)
        {
            throw new ApplicationException($"Author with Id: {updatedAuthor.Id} not found");
        }

        author.Update(updatedAuthor.Name, updatedAuthor.WebUrl);

        await SaveOrUpdate(author, CancellationToken.None);
    }

    public async Task<bool> DeleteByAuthor(Guid id)
    {
        return await DeleteItem(id, CancellationToken.None);
    }
}