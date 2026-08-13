using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public sealed class Series : Entity, IAuditableEntity
{
    private Series() { }
    private Series(Guid id, string slug, string title) : base(id) => (Slug, Title) = (slug, title);

    public string Slug { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public static Result<Series> Create(Guid id, string slug, string title)
    {
        var validation = Validate(slug, title);
        return validation.IsFailure ? Result.Failure<Series>(validation.Error) : new Series(id, slug, title);
    }

    public Result Update(string slug, string title)
    {
        var validation = Validate(slug, title);
        if (validation.IsFailure) return validation;
        (Slug, Title) = (slug, title);
        return Result.Success();
    }

    private static Result Validate(string slug, string title) =>
        string.IsNullOrWhiteSpace(slug) || slug.Length > 50 || string.IsNullOrWhiteSpace(title) || title.Length > 300
            ? Result.Failure(DomainErrors.Shelf.InvalidData("Series slug/title is empty or too long"))
            : Result.Success();
}
