using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public sealed class Author : Entity, IAuditableEntity
{
    private Author() { }
    private Author(Guid id, string slug, string name, string? nameEn) : base(id)
    {
        Slug = slug;
        NormalizedName = name.ToUpperInvariant();
        Name = name;
        NameEn = nameEn;
    }

    public string Slug { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? NameEn { get; private set; }
    public string NormalizedName { get; private set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public static Result<Author> Create(Guid id, string slug, string name, string? nameEn)
    {
        string normalizedInput = Normalize(name);
        var validation = Validate(slug, normalizedInput, nameEn);
        return validation.IsFailure
            ? Result.Failure<Author>(validation.Error)
            : new Author(id, slug, normalizedInput, nameEn);
    }

    public Result Update(string slug, string name, string? nameEn)
    {
        string normalizedInput = Normalize(name);
        var validation = Validate(slug, normalizedInput, nameEn);
        if (validation.IsFailure)
        {
            return validation;
        }

        (Slug, NormalizedName, Name, NameEn) = (slug, normalizedInput.ToUpperInvariant(), normalizedInput, nameEn);
        return Result.Success();
    }

    private static Result Validate(string slug, string name, string? nameEn) =>
        string.IsNullOrWhiteSpace(slug) || slug.Length > 50
        || string.IsNullOrWhiteSpace(name) || name.Length > 300
        || nameEn?.Length > 300
            ? Result.Failure(DomainErrors.Shelf.InvalidData("Author slug/name is empty or too long"))
            : Result.Success();

    private static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
