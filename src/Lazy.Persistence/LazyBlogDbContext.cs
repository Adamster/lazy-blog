using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence;

public class LazyBlogDbContext : DbContext
{
    public LazyBlogDbContext(DbContextOptions options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
}