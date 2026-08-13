using FluentValidation;
using Lazy.Application.Abstractions.File;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Shelf;

public record ManageBookData(Guid GenreId, Guid AuthorId, string Slug, string Title, Guid? SeriesId, int? VolumeNumber,
    string CoverUrl, decimal Rating, int RatingCount, string RatingSource, string? SourceWorkId, string? SourceUrl);

public sealed class ManageBookDataValidator : AbstractValidator<ManageBookData>
{
    public ManageBookDataValidator(bool requireCoverUrl = true)
    {
        RuleFor(x => x.GenreId).NotEmpty();
        RuleFor(x => x.AuthorId).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        if (requireCoverUrl) RuleFor(x => x.CoverUrl).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Rating).InclusiveBetween(0, 10);
        RuleFor(x => x.RatingCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RatingSource).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SourceWorkId).MaximumLength(100);
        RuleFor(x => x.SourceUrl).MaximumLength(1000);
        RuleFor(x => x.VolumeNumber).GreaterThan(0).When(x => x.VolumeNumber.HasValue);
        RuleFor(x => x)
            .Must(x => x.SeriesId.HasValue == x.VolumeNumber.HasValue)
            .WithMessage("A book must have both a series and a volume number, or neither");
    }
}

internal static class ShelfInput
{
    internal static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    internal static Result Apply(Book book, ManageBookData data)
    {
        var result = book.UpdateSlug(Normalize(data.Slug));
        if (result.IsFailure) return result;
        result = book.UpdateCatalog(data.GenreId, data.AuthorId, Normalize(data.Title), data.CoverUrl,
            data.SeriesId, data.VolumeNumber, data.SourceWorkId, data.SourceUrl);
        return result.IsFailure
            ? result
            : book.UpdateRating(data.Rating, data.RatingCount, data.RatingSource, DateTime.UtcNow);
    }

    internal static async Task<Error?> Relations(
        IGenreRepository genres,
        IAuthorRepository authors,
        ISeriesRepository series,
        ManageBookData data,
        CancellationToken ct)
    {
        if (await genres.GetAsync(data.GenreId, ct) is null) return DomainErrors.Shelf.GenreNotFound(data.GenreId);
        if (await authors.GetAsync(data.AuthorId, ct) is null) return DomainErrors.Shelf.AuthorNotFound(data.AuthorId);
        if (data.SeriesId is { } seriesId && await series.GetAsync(seriesId, ct) is null)
            return DomainErrors.Shelf.SeriesNotFound(seriesId);
        return null;
    }
}
