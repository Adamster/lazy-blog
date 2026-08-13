using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public sealed class Genre : Entity, IAuditableEntity
{
    private Genre() { }
    private Genre(Guid id, string slug, string name, int sortOrder) : base(id) =>
        (Slug, Name, SortOrder) = (slug, name, sortOrder);

    public string Slug { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public static Result<Genre> Create(Guid id, string slug, string name, int sortOrder)
    {
        var validation = Validate(slug, name);
        return validation.IsFailure ? Result.Failure<Genre>(validation.Error) : new Genre(id, slug, name, sortOrder);
    }

    public Result Update(string slug, string name, int sortOrder)
    {
        var validation = Validate(slug, name);
        if (validation.IsFailure) return validation;
        (Slug, Name, SortOrder) = (slug, name, sortOrder);
        return Result.Success();
    }

    private static Result Validate(string slug, string name) =>
        string.IsNullOrWhiteSpace(slug) || slug.Length > 50 || string.IsNullOrWhiteSpace(name) || name.Length > 100
            ? Result.Failure(DomainErrors.Shelf.InvalidData("Genre slug/name is empty or too long"))
            : Result.Success();
}
