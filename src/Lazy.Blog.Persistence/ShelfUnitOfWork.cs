using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence;

public sealed class ShelfUnitOfWork(LazyBlogDbContext dbContext) : IShelfUnitOfWork
{
    public async Task<Result> SaveChangesAsync(ShelfConflict? conflict, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateException exception) when (conflict is not null && Matches(conflict.Value, exception))
        {
            return Result.Failure(conflict switch
            {
                ShelfConflict.Book => DomainErrors.Shelf.BookAlreadyExists,
                ShelfConflict.Author => DomainErrors.Shelf.AuthorAlreadyExists,
                ShelfConflict.Genre => DomainErrors.Shelf.GenreAlreadyExists,
                ShelfConflict.AuthorInUse => DomainErrors.Shelf.AuthorInUse,
                ShelfConflict.GenreInUse => DomainErrors.Shelf.GenreInUse,
                _ => throw new ArgumentOutOfRangeException(nameof(conflict))
            });
        }
    }

    private static bool Matches(ShelfConflict conflict, DbUpdateException exception) => conflict switch
    {
        ShelfConflict.AuthorInUse or ShelfConflict.GenreInUse => IsForeignKeyViolation(exception),
        _ => IsUniqueConstraint(exception)
    };

    private static bool IsUniqueConstraint(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static bool IsForeignKeyViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 547 };
}
