using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public sealed class Author : Entity, IAuditableEntity
{
    private Author() { }
    private Author(Guid id, string name) : base(id) => SetName(name);

    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public static Result<Author> Create(Guid id, string name)
    {
        string normalized = Normalize(name);
        return IsValid(normalized)
            ? new Author(id, normalized)
            : Result.Failure<Author>(DomainErrors.Shelf.InvalidData("Author name must contain 1 to 300 characters"));
    }

    public Result Update(string name)
    {
        string normalized = Normalize(name);
        if (!IsValid(normalized))
            return Result.Failure(DomainErrors.Shelf.InvalidData("Author name must contain 1 to 300 characters"));
        SetName(normalized);
        return Result.Success();
    }

    private void SetName(string name)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    private static bool IsValid(string value) => value.Length is > 0 and <= 300;
    private static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
