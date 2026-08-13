using Lazy.Application.Shelf;
using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.User;
using Lazy.Persistence;
using Lazy.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lazy.Blog.Tests.Application.Shelf;

public sealed class GetUserShelfQueryHandlerTests
{
    private static LazyBlogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LazyBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LazyBlogDbContext(options);
    }

    private static Book CreateBook(
        Guid genreId, Guid authorId, string slug, string title, Guid? seriesId, int? volumeNumber)
    {
        var book = Book.Create(Guid.NewGuid(), genreId, authorId, slug, title, "https://cover").Value;
        book.UpdateCatalog(genreId, authorId, title, "https://cover", seriesId, volumeNumber, null, null);
        book.UpdateRating(8, 100, "FantLab", DateTime.UtcNow);
        return book;
    }

    private static User CreateUser(string userName)
    {
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create($"{userName}@test.local").Value,
            DisplayName.Create(userName).Value,
            UserName.Create(userName).Value,
            null);
        user.NormalizedUserName = userName.ToUpperInvariant();
        return user;
    }

    private static GetUserShelfQueryHandler CreateHandler(LazyBlogDbContext context) =>
        new(new BookRepository(context), new UserBookStatusRepository(context), new RatingSyncRunRepository(context));

    [Fact]
    public async Task ReadingOneVolumePullsInWholeSeriesOnPersonalShelf()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var series = Series.Create(Guid.NewGuid(), "cycle", "Cycle").Value;
        var volume1 = CreateBook(genre.Id, author.Id, "vol-1", "Volume 1", series.Id, 1);
        var volume2 = CreateBook(genre.Id, author.Id, "vol-2", "Volume 2", series.Id, 2);
        var user = CreateUser("reader");
        context.AddRange(genre, author, series, volume1, volume2, user);
        context.Add(UserBookStatus.Create(user.Id, volume1.Id, ReadingStatus.Reading));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context).Handle(
            new GetUserShelfQuery("reader", "all", null, null, null, 0, 24), TestContext.Current.CancellationToken);

        var card = Assert.Single(result.Value.Items.OfType<ShelfSeriesCard>());
        Assert.Equal(2, card.BooksCount);
        Assert.Equal(0, card.ReadCount);
        Assert.Equal(1, card.ReadingCount);
    }

    [Fact]
    public async Task StatusAllIncludesBothReadingAndRead()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var reading = CreateBook(genre.Id, author.Id, "reading", "Reading Book", null, null);
        var read = CreateBook(genre.Id, author.Id, "read", "Read Book", null, null);
        var user = CreateUser("reader");
        context.AddRange(genre, author, reading, read, user);
        context.AddRange(
            UserBookStatus.Create(user.Id, reading.Id, ReadingStatus.Reading),
            UserBookStatus.Create(user.Id, read.Id, ReadingStatus.Read));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context).Handle(
            new GetUserShelfQuery("reader", "all", null, null, null, 0, 24), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Value.Items.OfType<ShelfBookCard>().Count());
    }

    [Fact]
    public async Task RecentSortOrdersByMostRecentStatusChange()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var older = CreateBook(genre.Id, author.Id, "older", "Older Book", null, null);
        var newer = CreateBook(genre.Id, author.Id, "newer", "Newer Book", null, null);
        var user = CreateUser("reader");
        var olderStatus = UserBookStatus.Create(user.Id, older.Id, ReadingStatus.Read);
        olderStatus.CreatedOnUtc = DateTime.UtcNow.AddDays(-10);
        var newerStatus = UserBookStatus.Create(user.Id, newer.Id, ReadingStatus.Read);
        newerStatus.CreatedOnUtc = DateTime.UtcNow;
        context.AddRange(genre, author, older, newer, user, olderStatus, newerStatus);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context).Handle(
            new GetUserShelfQuery("reader", "all", null, null, "recent", 0, 24), TestContext.Current.CancellationToken);

        var cards = result.Value.Items.OfType<ShelfBookCard>().ToList();
        Assert.Equal("Newer Book", cards[0].Title);
        Assert.Equal("Older Book", cards[1].Title);
    }
}
