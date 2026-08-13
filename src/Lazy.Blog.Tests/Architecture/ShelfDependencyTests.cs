using System.Reflection;
using Lazy.Application.Shelf;
using Xunit;

namespace Lazy.Blog.Tests.Architecture;

public sealed class ShelfDependencyTests
{
    [Fact]
    public void ShelfApplicationMustNotReferenceEntityFrameworkCore()
    {
        var references = typeof(GetBooksQuery).Assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(references, reference => reference.Name?.StartsWith("Microsoft.EntityFrameworkCore") == true);
    }

    [Fact]
    public void ShelfRequestsMustNotExposeAspNetHttpTypes()
    {
        var shelfTypes = typeof(GetBooksQuery).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("Lazy.Application.Shelf") == true);

        var exposedTypes = shelfTypes.SelectMany(type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => property.PropertyType)
                    .Concat(type.GetConstructors().SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType))))
            .ToList();

        Assert.DoesNotContain(exposedTypes, type => type.FullName?.StartsWith("Microsoft.AspNetCore.Http") == true);
    }
}
