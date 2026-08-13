using Lazy.Domain.Entities;
using Xunit;

namespace Lazy.Blog.Tests.Domain;

public sealed class BookTests
{
    [Fact]
    public void CreateRejectsInvalidCatalogData()
    {
        var result = Book.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), string.Empty, "Title", "https://cover");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void UpdateRatingRejectsValuesOutsideSupportedRange()
    {
        var book = Book.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "slug", "Title", "https://cover").Value;
        var result = book.UpdateRating(10.01m, 1, "FantLab", DateTime.UtcNow);
        Assert.True(result.IsFailure);
    }
}
