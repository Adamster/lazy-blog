using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class GenreRepository(LazyBlogDbContext dbContext) : IGenreRepository
{
    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken ct) =>
        await dbContext.Set<Genre>().AsNoTracking().OrderBy(genre => genre.SortOrder).ToListAsync(ct);

    public Task<Genre?> GetAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Genre>().FirstOrDefaultAsync(genre => genre.Id == id, ct);

    public Task<bool> ConflictsAsync(Guid? excludedId, string slug, string name, CancellationToken ct) =>
        dbContext.Set<Genre>().AnyAsync(
            genre => genre.Id != excludedId &&
                (genre.Slug == slug || genre.Name == name),
            ct);

    public void Add(Genre genre) => dbContext.Add(genre);

    public void Remove(Genre genre) => dbContext.Remove(genre);
}
