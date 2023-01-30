using Lazy.DataContracts.Author;
using Lazy.Domain;
using Lazy.Infrastructure;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Repository;

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(LazyBlogDbContext dbContext) : base(dbContext)
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
        return result != null
            ? new AuthorItemDto(
                result.Id,
                result.Name,
                result.WebUrl,
                result.Email)
            : null;
    }

    public async Task<AuthorItemDto> CreateAuthor(AuthorItemDto authorItemDto)
    {
        var author = new Author(authorItemDto.Name, authorItemDto.WebUrl, authorItemDto.AuthorEmail);

        var createdAuthor = await SaveOrUpdate(author, CancellationToken.None);
        return new AuthorItemDto(createdAuthor.Id, createdAuthor.Name, createdAuthor.WebUrl, createdAuthor.Email);
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

    public async Task<AuthorItemDto> GetByEmail(string email)
    {
        var author = await DbContext.Authors.SingleAsync(x => x.Email == email);
        return new AuthorItemDto(author.Id, author.Name, author.WebUrl, author.Email);
    }
}