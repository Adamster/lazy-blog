using Microsoft.EntityFrameworkCore;

namespace Lazy.Infrastructure;

public static class DbInitializer
{
    public static void Initialize(LazyBlogDbContext context)
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}