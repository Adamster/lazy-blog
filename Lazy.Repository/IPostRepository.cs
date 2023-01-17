using Lazy.Domain;
using Lazy.Infrastructure;

namespace Lazy.Repository;

public interface IPostRepository : IRepository<Post>
{
}


public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(LazyBlogDbContext lazyBlogDbContext) : base(lazyBlogDbContext)
    {
    }
}