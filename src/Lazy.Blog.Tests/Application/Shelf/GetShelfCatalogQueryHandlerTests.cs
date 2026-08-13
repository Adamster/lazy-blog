using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Shelf;
using Lazy.Domain.Entities;
using Lazy.Persistence;
using Lazy.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lazy.Blog.Tests.Application.Shelf;

public sealed class GetShelfCatalogQueryHandlerTests
{
    private static LazyBlogDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LazyBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LazyBlogDbContext(options);
    }

    private static Book CreateBook(
        Guid genreId, Guid authorId, string slug, string title, Guid? seriesId, int? volumeNumber,
        decimal rating, int ratingCount)
    {
        var book = Book.Create(Guid.NewGuid(), genreId, authorId, slug, title, "https://cover").Value;
        book.UpdateCatalog(genreId, authorId, title, "https://cover", seriesId, volumeNumber, null, null);
        book.UpdateRating(rating, ratingCount, "FantLab", DateTime.UtcNow);
        return book;
    }

    private static GetShelfCatalogQueryHandler CreateHandler(LazyBlogDbContext context, Guid userId) =>
        new(new BookRepository(context), new UserBookStatusRepository(context), new RatingSyncRunRepository(context),
            new FakeCurrentUserContext(userId));

    private sealed class FakeCurrentUserContext(Guid userId) : ICurrentUserContext
    {
        public bool IsCurrentUser(Guid id) => id == userId;
        public Guid GetCurrentUserId() => userId;
    }

    [Fact]
    public async Task SeriesWithMultipleVolumesAppearsAsOneCardNotDuplicated()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var series = Series.Create(Guid.NewGuid(), "cycle", "Cycle").Value;
        context.AddRange(genre, author, series);
        context.AddRange(
            CreateBook(genre.Id, author.Id, "vol-1", "Volume 1", series.Id, 1, 8, 100),
            CreateBook(genre.Id, author.Id, "vol-2", "Volume 2", series.Id, 2, 9, 50));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context, Guid.NewGuid()).Handle(
            new GetShelfCatalogQuery(null, null, null, null, 0, 24), TestContext.Current.CancellationToken);

        var seriesCards = result.Value.Items.OfType<ShelfSeriesCard>().ToList();
        Assert.Single(seriesCards);
        Assert.Equal(2, seriesCards[0].BooksCount);
        Assert.Empty(result.Value.Items.OfType<ShelfBookCard>());
        Assert.Equal(2, result.Value.TotalBooks);
    }

    [Fact]
    public async Task SeriesCardAggregatesStatusAcrossVolumes()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var series = Series.Create(Guid.NewGuid(), "cycle", "Cycle").Value;
        var volume1 = CreateBook(genre.Id, author.Id, "vol-1", "Volume 1", series.Id, 1, 8, 100);
        var volume2 = CreateBook(genre.Id, author.Id, "vol-2", "Volume 2", series.Id, 2, 9, 50);
        context.AddRange(genre, author, series, volume1, volume2);
        var userId = Guid.NewGuid();
        context.AddRange(
            UserBookStatus.Create(userId, volume1.Id, ReadingStatus.Read),
            UserBookStatus.Create(userId, volume2.Id, ReadingStatus.Reading));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context, userId).Handle(
            new GetShelfCatalogQuery(null, null, null, null, 0, 24), TestContext.Current.CancellationToken);

        var card = Assert.Single(result.Value.Items.OfType<ShelfSeriesCard>());
        Assert.Equal(1, card.ReadCount);
        Assert.Equal(1, card.ReadingCount);
        Assert.Equal("reading", card.Status);
    }

    [Fact]
    public async Task GenreFilterPullsInWholeSeriesNotJustMatchingVolume()
    {
        await using var context = CreateContext();
        var matchingGenre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var otherGenre = Genre.Create(Guid.NewGuid(), "scifi", "Sci-Fi", 2).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var series = Series.Create(Guid.NewGuid(), "cycle", "Cycle").Value;
        context.AddRange(matchingGenre, otherGenre, author, series);
        context.AddRange(
            CreateBook(matchingGenre.Id, author.Id, "vol-1", "Volume 1", series.Id, 1, 8, 100),
            CreateBook(otherGenre.Id, author.Id, "vol-2", "Volume 2", series.Id, 2, 9, 50));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context, Guid.NewGuid()).Handle(
            new GetShelfCatalogQuery("fantasy", null, null, null, 0, 24), TestContext.Current.CancellationToken);

        var card = Assert.Single(result.Value.Items.OfType<ShelfSeriesCard>());
        Assert.Equal(2, card.BooksCount);
    }

    [Fact]
    public async Task AuthorFilterPullsInWholeSeriesNotJustMatchingVolume()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var matchingAuthor = Author.Create(Guid.NewGuid(), "matching-author", "Matching Author").Value;
        var otherAuthor = Author.Create(Guid.NewGuid(), "other-author", "Other Author").Value;
        var series = Series.Create(Guid.NewGuid(), "cycle", "Cycle").Value;
        context.AddRange(genre, matchingAuthor, otherAuthor, series);
        context.AddRange(
            CreateBook(genre.Id, matchingAuthor.Id, "vol-1", "Volume 1", series.Id, 1, 8, 100),
            CreateBook(genre.Id, otherAuthor.Id, "vol-2", "Volume 2", series.Id, 2, 9, 50));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context, Guid.NewGuid()).Handle(
            new GetShelfCatalogQuery(null, "matching-author", null, null, 0, 24), TestContext.Current.CancellationToken);

        var card = Assert.Single(result.Value.Items.OfType<ShelfSeriesCard>());
        Assert.Equal(2, card.BooksCount);
    }

    [Fact]
    public async Task RatingSortUsesAggregatedSeriesRatingNotSingleVolume()
    {
        await using var context = CreateContext();
        var genre = Genre.Create(Guid.NewGuid(), "fantasy", "Fantasy", 1).Value;
        var author = Author.Create(Guid.NewGuid(), "author", "Author").Value;
        var lowSeries = Series.Create(Guid.NewGuid(), "low-cycle", "Low Cycle").Value;
        var highSeries = Series.Create(Guid.NewGuid(), "high-cycle", "High Cycle").Value;
        context.AddRange(genre, author, lowSeries, highSeries);
        // low-cycle: weighted average is low (9-rated volume has far fewer raters than the 2-rated one)
        context.AddRange(
            CreateBook(genre.Id, author.Id, "low-1", "Low 1", lowSeries.Id, 1, 9, 5),
            CreateBook(genre.Id, author.Id, "low-2", "Low 2", lowSeries.Id, 2, 2, 500),
            CreateBook(genre.Id, author.Id, "high-1", "High 1", highSeries.Id, 1, 8, 500),
            CreateBook(genre.Id, author.Id, "high-2", "High 2", highSeries.Id, 2, 8, 500));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await CreateHandler(context, Guid.NewGuid()).Handle(
            new GetShelfCatalogQuery(null, null, null, "rating", 0, 24), TestContext.Current.CancellationToken);

        var cards = result.Value.Items.OfType<ShelfSeriesCard>().ToList();
        Assert.Equal("High Cycle", cards[0].Title);
        Assert.Equal("Low Cycle", cards[1].Title);
        Assert.True(cards[0].Rating > cards[1].Rating);
    }
}
