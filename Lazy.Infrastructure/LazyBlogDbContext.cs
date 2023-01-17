using Lazy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Infrastructure;

public class LazyBlogDbContext : DbContext
{
    public LazyBlogDbContext(DbContextOptions<LazyBlogDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LazyBlogDbContext).Assembly);
    }

    public DbSet<Author> Authors { get; set; } = null!;

    public DbSet<Post> Posts { get; set; } = null!;
}