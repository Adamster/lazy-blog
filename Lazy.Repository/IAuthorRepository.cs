using Lazy.Domain;
using Lazy.Infrastructure;
using Lazy.Repository.Models.Author;
using Mapster;

namespace Lazy.Repository;

public interface IAuthorRepository : IRepository<Author>
{
    Task<IList<AuthorDto>> GetAll();
    Task<AuthorDto?> GetById(Guid id);
}

public class AuthorRepository : Repository<Author>, IAuthorRepository
{
    public AuthorRepository(LazyBlogDbContext lazyBlogDbContext) : base(lazyBlogDbContext)
    {
    }

    public async Task<IList<AuthorDto>> GetAll()
    {
        var items =  await GetItems();
        return items.Adapt<IList<AuthorDto>>();
    }

    public async Task<AuthorDto?> GetById(Guid id)
    {
        var result = await GetItemById(id);
        return result?.Adapt<AuthorDto>();
    }
}