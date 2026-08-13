using Lazy.Domain.Entities;
using Xunit;

namespace Lazy.Blog.Tests.Domain;

public sealed class SeriesTests
{
    [Fact]
    public void CreateRejectsEmptySlug()
    {
        var result = Series.Create(Guid.NewGuid(), string.Empty, "Title", "Title (EN)");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CreateAcceptsValidSlugAndTitle()
    {
        var result = Series.Create(Guid.NewGuid(), "slug", "Title", "Title (EN)");
        Assert.True(result.IsSuccess);
    }
}
