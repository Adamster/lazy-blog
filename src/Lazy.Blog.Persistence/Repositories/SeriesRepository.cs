using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence.Repositories;

public sealed class SeriesRepository(LazyBlogDbContext dbContext) : ISeriesRepository
{
    public async Task<IReadOnlyList<Series>> GetAllAsync(CancellationToken ct) =>
        await dbContext.Set<Series>().AsNoTracking().OrderBy(series => series.Title).ToListAsync(ct);

    public Task<Series?> GetAsync(Guid id, CancellationToken ct) =>
        dbContext.Set<Series>().FirstOrDefaultAsync(series => series.Id == id, ct);

    public Task<Series?> GetBySlugAsync(string slug, CancellationToken ct) =>
        dbContext.Set<Series>().AsNoTracking().FirstOrDefaultAsync(series => series.Slug == slug, ct);

    public Task<bool> ConflictsAsync(Guid? excludedId, string slug, string title, CancellationToken ct) =>
        dbContext.Set<Series>().AnyAsync(
            series => series.Id != excludedId &&
                (series.Slug == slug || series.Title == title),
            ct);

    public void Add(Series series) => dbContext.Add(series);

    public void Remove(Series series) => dbContext.Remove(series);
}
