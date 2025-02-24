using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence;

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