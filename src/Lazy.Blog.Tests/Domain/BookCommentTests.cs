using Lazy.Domain.Entities;
using Xunit;

namespace Lazy.Blog.Tests.Domain;

public sealed class BookCommentTests
{
    [Fact]
    public void CreateRejectsEmptyBody()
    {
        var result = BookComment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), string.Empty);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void CanEditIsTrueWithinEditWindow()
    {
        var comment = BookComment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Great book").Value;
        comment.CreatedOnUtc = DateTime.UtcNow;
        Assert.True(comment.CanEdit(DateTime.UtcNow));
    }

    [Fact]
    public void CanEditIsFalseAfterEditWindowExpires()
    {
        var comment = BookComment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Great book").Value;
        comment.CreatedOnUtc = DateTime.UtcNow - BookComment.EditWindow - TimeSpan.FromMinutes(1);
        Assert.False(comment.CanEdit(DateTime.UtcNow));
    }
}
