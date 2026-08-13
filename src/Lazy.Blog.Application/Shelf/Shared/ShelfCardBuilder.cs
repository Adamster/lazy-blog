using Lazy.Domain.Entities;

namespace Lazy.Application.Shelf;

internal readonly record struct BookStatusInfo(ReadingStatus? Status, DateTime? ChangedOnUtc);

internal static class ShelfCardBuilder
{
    internal static (IReadOnlyList<ShelfCatalogItem> Items, int TotalItems) Build(
        IReadOnlyList<Book> standalone,
        IReadOnlyList<IGrouping<Guid, Book>> seriesGroups,
        IReadOnlyDictionary<Guid, BookStatusInfo> statuses,
        string? sort,
        int offset,
        int limit)
    {
        var cards = new List<CatalogCard>();
        cards.AddRange(standalone.Select(book => ToBookCard(book, statuses)));
        cards.AddRange(seriesGroups.Select(group => ToSeriesCard(group, statuses)));

        var sorted = Sort(cards, sort);
        var page = sorted.Skip(offset).Take(limit).Select(card => card.Item).ToList();
        return (page, sorted.Count);
    }

    private static BookStatusInfo LookupStatus(IReadOnlyDictionary<Guid, BookStatusInfo> statuses, Guid bookId) =>
        statuses.TryGetValue(bookId, out var info) ? info : default;

    private static CatalogCard ToBookCard(Book book, IReadOnlyDictionary<Guid, BookStatusInfo> statuses)
    {
        var info = LookupStatus(statuses, book.Id);
        var card = new ShelfBookCard(
            book.Id,
            book.Slug,
            book.Title,
            new BookAuthorResponse(book.Author.Id, book.Author.Slug, book.Author.Name),
            new BookGenreResponse(book.Genre.Id, book.Genre.Slug, book.Genre.Name),
            book.CoverUrl,
            book.Rating,
            book.RatingCount,
            info.Status is null ? null : ShelfMapping.StatusName(info.Status.Value));
        return new CatalogCard(card, book.Author.Name, book.Title, book.Rating, book.RatingCount, info.ChangedOnUtc);
    }

    private static CatalogCard ToSeriesCard(IGrouping<Guid, Book> group, IReadOnlyDictionary<Guid, BookStatusInfo> statuses)
    {
        var volumes = group.OrderBy(book => book.VolumeNumber).ToList();
        var first = volumes[0];
        var volumeStatuses = volumes.Select(book => LookupStatus(statuses, book.Id)).ToList();
        int readCount = volumeStatuses.Count(info => info.Status == ReadingStatus.Read);
        int readingCount = volumeStatuses.Count(info => info.Status == ReadingStatus.Reading);
        int readersCount = volumes.Sum(book => book.RatingCount);
        decimal rating = readersCount == 0 ? 0 : volumes.Sum(book => book.Rating * book.RatingCount) / readersCount;
        string? status = readingCount > 0 ? "reading" : readCount == volumes.Count ? "read" : null;
        DateTime? recentUtc = volumeStatuses.Select(info => info.ChangedOnUtc).Where(value => value is not null).MaxBy(value => value);

        var card = new ShelfSeriesCard(
            group.Key,
            first.Series!.Slug,
            first.Series.Title,
            new BookAuthorResponse(first.Author.Id, first.Author.Slug, first.Author.Name),
            volumes.Take(3).Select(book => book.CoverUrl).ToList(),
            volumes.Count,
            readCount,
            readingCount,
            rating,
            readersCount,
            status);
        return new CatalogCard(card, first.Author.Name, first.Series.Title, rating, readersCount, recentUtc);
    }

    private static List<CatalogCard> Sort(List<CatalogCard> cards, string? sort) => sort switch
    {
        "rating" => cards.OrderByDescending(card => card.Rating).ThenBy(card => card.AuthorName).ThenBy(card => card.SortTitle).ToList(),
        "readers" => cards.OrderByDescending(card => card.ReadersCount).ThenByDescending(card => card.Rating)
            .ThenBy(card => card.SortTitle).ToList(),
        "author" => cards.OrderBy(card => card.AuthorName).ThenBy(card => card.SortTitle).ToList(),
        "title" => cards.OrderBy(card => card.SortTitle).ThenBy(card => card.AuthorName).ToList(),
        "recent" => cards.OrderByDescending(card => card.RecentUtc ?? DateTime.MinValue).ThenBy(card => card.SortTitle).ToList(),
        _ => cards.OrderByDescending(card => (card.Rating * card.ReadersCount + 700m) / (card.ReadersCount + 100m))
            .ThenByDescending(card => card.ReadersCount).ThenBy(card => card.AuthorName).ThenBy(card => card.SortTitle).ToList()
    };

    private sealed record CatalogCard(
        ShelfCatalogItem Item, string AuthorName, string SortTitle, decimal Rating, int ReadersCount, DateTime? RecentUtc);
}
