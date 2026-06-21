using Microsoft.OpenApi;

namespace Lazy.Blog.Api.Extensions;

public static class OpenApi
{
    public static void AddOpenApi(this IServiceCollection services)
    {
        OpenApiServiceCollectionExtensions.AddOpenApi(services, "v1", options =>
            options.AddSchemaTransformer((schema, _, _) =>
            {
                if (schema.Type is { } type
                    && type.HasFlag(JsonSchemaType.Integer)
                    && type.HasFlag(JsonSchemaType.String))
                {
                    schema.Type = type & ~JsonSchemaType.String;
                }

                return Task.CompletedTask;
            }));
    }
}
