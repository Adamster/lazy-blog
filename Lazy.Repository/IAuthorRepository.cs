using Lazy.DataContracts.Author;
using Lazy.Domain;
using Lazy.Infrastructure;
using Mapster;

namespace Lazy.Repository;

public interface IAuthorRepository : IRepository<Author>
{
    Task<IList<AuthorItemDto>> GetAll();
    Task<AuthorItemDto?> GetById(Guid id);
    Task<AuthorItemDto> CreateAuthor(AuthorItemDto AuthorItemDto);
    Task UpdateAuthor(AuthorItemDto updatedAuthor);
    Task<bool> DeleteByAuthor(Guid id);
}

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(LazyBlogDbContext lazyBlogDbContext) : base(lazyBlogDbContext)
    {
    }

    public async Task<IList<AuthorItemDto>> GetAll()
    {
        var items = await GetItems();
        return items.Adapt<IList<AuthorItemDto>>();
    }

    public async Task<AuthorItemDto?> GetById(Guid id)
    {
        var result = await GetItemById(id);
        return result?.Adapt<AuthorItemDto>();
    }

    public async Task<AuthorItemDto> CreateAuthor(AuthorItemDto authorItemDto)
    {
        var author = new Author(authorItemDto.Name, authorItemDto.WebUrl);

        var createdAuthor = await SaveOrUpdate(author, CancellationToken.None);
        return createdAuthor.Adapt<AuthorItemDto>();
    }

    public async Task UpdateAuthor(AuthorItemDto updatedAuthor)
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