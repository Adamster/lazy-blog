using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public interface IShelfRepository
{
    IQueryable<Genre> Genres { get; }
    IQueryable<Book> Books { get; }
    IQueryable<Author> Authors { get; }
    IQueryable<UserBookStatus> UserBookStatuses { get; }
    IQueryable<BookComment> BookComments { get; }
    IQueryable<RatingSyncRun> RatingSyncRuns { get; }
    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
}
