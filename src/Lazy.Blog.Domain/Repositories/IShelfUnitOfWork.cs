using Lazy.Domain.Shared;

namespace Lazy.Domain.Repositories;

public sealed record Page<T>(IReadOnlyList<T> Items, int Total);

public enum ShelfConflict { Book, Author, Genre, Series, AuthorInUse, GenreInUse, SeriesInUse }

public interface IShelfUnitOfWork
{
    Task<Result> SaveChangesAsync(ShelfConflict? conflict, CancellationToken ct);
}
