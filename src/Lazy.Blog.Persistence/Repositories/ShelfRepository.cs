using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class ShelfRepository(LazyBlogDbContext dbContext) : IShelfRepository
{
    public IQueryable<Genre> Genres => dbContext.Set<Genre>();
    public IQueryable<Book> Books => dbContext.Set<Book>();
    public IQueryable<Author> Authors => dbContext.Set<Author>();
    public IQueryable<UserBookStatus> UserBookStatuses => dbContext.Set<UserBookStatus>();
    public IQueryable<BookComment> BookComments => dbContext.Set<BookComment>();
    public IQueryable<RatingSyncRun> RatingSyncRuns => dbContext.Set<RatingSyncRun>();
    public void Add<T>(T entity) where T : class => dbContext.Set<T>().Add(entity);
    public void Remove<T>(T entity) where T : class => dbContext.Set<T>().Remove(entity);
}
