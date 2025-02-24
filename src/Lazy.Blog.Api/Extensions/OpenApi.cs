namespace Lazy.App.Extensions;

public static class OpenApi
{
    public static void AddOpenApi(this IServiceCollection services)
    {
        OpenApiServiceCollectionExtensions.AddOpenApi(services);
    }
}