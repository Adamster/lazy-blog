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
        catch (DbUpdateException exception) when (conflict is not null && IsUniqueConstraint(exception))
        {
            return Result.Failure(conflict switch
            {
                ShelfConflict.Book => DomainErrors.Shelf.BookAlreadyExists,
                ShelfConflict.Author => DomainErrors.Shelf.AuthorAlreadyExists,
                ShelfConflict.Genre => DomainErrors.Shelf.GenreAlreadyExists,
                _ => throw new ArgumentOutOfRangeException(nameof(conflict))
            });
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
