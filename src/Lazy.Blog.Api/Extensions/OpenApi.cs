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

                // The native OpenApi polymorphism support emits `anyOf` for [JsonPolymorphic] hierarchies,
                // but TS codegen tools only build a clean discriminated union for `oneOf` + discriminator;
                // with `anyOf` they instead merge all branches' `required` into one type, so a card missing
                // another variant's fields fails to satisfy the union. Fix up to `oneOf`, which is what a
                // discriminated JsonDerivedType hierarchy actually means.
                if (schema.Discriminator is not null && schema.AnyOf is { Count: > 0 } anyOf)
                {
                    schema.OneOf = anyOf;
                    schema.AnyOf = null;
                }

                return Task.CompletedTask;
            }));
    }
}
