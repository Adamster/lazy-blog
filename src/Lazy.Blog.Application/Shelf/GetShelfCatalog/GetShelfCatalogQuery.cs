using FluentValidation;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record GetShelfCatalogQuery(
    string? Genre,
    string? AuthorSlug,
    string? Search,
    string? Sort,
    int Offset,
    int Limit) : IQuery<ShelfCatalogResponse>;

public sealed class GetShelfCatalogQueryValidator : AbstractValidator<GetShelfCatalogQuery>
{
    public GetShelfCatalogQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 200);
    }
}

public sealed class GetShelfCatalogQueryHandler(
    IBookRepository books,
    IUserBookStatusRepository userBookStatuses,
    IRatingSyncRunRepository ratingSyncRuns,
    ICurrentUserContext currentUser) : IQueryHandler<GetShelfCatalogQuery, ShelfCatalogResponse>
{
    public async Task<Result<ShelfCatalogResponse>> Handle(GetShelfCatalogQuery request, CancellationToken ct)
    {
        int offset = Math.Max(0, request.Offset);
        int limit = Math.Clamp(request.Limit, 1, 200);

        var filtered = await books.GetFilteredAsync(request.Genre, request.AuthorSlug, request.Search, ct);
        var standalone = filtered.Where(book => book.SeriesId is null).ToList();
        var matchingSeriesIds = filtered
            .Where(book => book.SeriesId is not null)
            .Select(book => book.SeriesId!.Value)
            .Distinct()
            .ToList();
        var seriesGroups = matchingSeriesIds.Count == 0
            ? []
            : (await books.GetBySeriesIdsAsync(matchingSeriesIds, ct))
                .GroupBy(book => book.SeriesId!.Value)
                .ToList();

        var allBookIds = standalone.Select(book => book.Id)
            .Concat(seriesGroups.SelectMany(group => group.Select(book => book.Id)))
            .ToList();
        var statuses = await userBookStatuses.GetStatusesAsync(currentUser.GetCurrentUserId(), allBookIds, ct);

        var cards = new List<CatalogCard>();
        cards.AddRange(standalone.Select(book => ToBookCard(book, statuses)));
        cards.AddRange(seriesGroups.Select(group => ToSeriesCard(group, statuses)));

        var sorted = Sort(cards, request.Sort);
        int totalItems = sorted.Count;
        int totalBooks = standalone.Count + seriesGroups.Sum(group => group.Count());
        var page = sorted.Skip(offset).Take(limit).Select(card => card.Item).ToList();

        return new ShelfCatalogResponse(page, totalItems, totalBooks, offset, limit, await ratingSyncRuns.GetLastSyncAsync(ct));
    }

    private static CatalogCard ToBookCard(Book book, IReadOnlyDictionary<Guid, ReadingStatus> statuses)
    {
        var status = ShelfMapping.LookupStatus(statuses, book.Id);
        var card = new ShelfBookCard(
            book.Id,
            book.Slug,
            book.Title,
            new BookAuthorResponse(book.Author.Id, book.Author.Slug, book.Author.Name),
            new BookGenreResponse(book.Genre.Id, book.Genre.Slug, book.Genre.Name),
            book.CoverUrl,
            book.Rating,
            book.RatingCount,
            status is null ? null : ShelfMapping.StatusName(status.Value));
        return new CatalogCard(card, book.Author.Name, book.Title, book.Rating, book.RatingCount);
    }

    private static CatalogCard ToSeriesCard(IGrouping<Guid, Book> group, IReadOnlyDictionary<Guid, ReadingStatus> statuses)
    {
        var volumes = group.OrderBy(book => book.VolumeNumber).ToList();
        var first = volumes[0];
        int readCount = volumes.Count(book => ShelfMapping.LookupStatus(statuses, book.Id) == ReadingStatus.Read);
        int readingCount = volumes.Count(book => ShelfMapping.LookupStatus(statuses, book.Id) == ReadingStatus.Reading);
        int readersCount = volumes.Sum(book => book.RatingCount);
        decimal rating = readersCount == 0 ? 0 : volumes.Sum(book => book.Rating * book.RatingCount) / readersCount;
        string? status = readingCount > 0 ? "reading" : readCount == volumes.Count ? "read" : null;

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
        return new CatalogCard(card, first.Author.Name, first.Series.Title, rating, readersCount);
    }

    private static List<CatalogCard> Sort(List<CatalogCard> cards, string? sort) => sort switch
    {
        "rating" => cards.OrderByDescending(card => card.Rating).ThenBy(card => card.AuthorName).ThenBy(card => card.SortTitle).ToList(),
        "readers" => cards.OrderByDescending(card => card.ReadersCount).ThenByDescending(card => card.Rating)
            .ThenBy(card => card.SortTitle).ToList(),
        "author" => cards.OrderBy(card => card.AuthorName).ThenBy(card => card.SortTitle).ToList(),
        "title" => cards.OrderBy(card => card.SortTitle).ThenBy(card => card.AuthorName).ToList(),
        _ => cards.OrderByDescending(card => (card.Rating * card.ReadersCount + 700m) / (card.ReadersCount + 100m))
            .ThenByDescending(card => card.ReadersCount).ThenBy(card => card.AuthorName).ThenBy(card => card.SortTitle).ToList()
    };

    private sealed record CatalogCard(ShelfCatalogItem Item, string AuthorName, string SortTitle, decimal Rating, int ReadersCount);
}
