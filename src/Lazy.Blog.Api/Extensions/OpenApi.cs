namespace Lazy.Blog.Api.Extensions;

public static class OpenApi
{
    public static void AddOpenApi(this IServiceCollection services)
    {
        OpenApiServiceCollectionExtensions.AddOpenApi(services);
    }
}