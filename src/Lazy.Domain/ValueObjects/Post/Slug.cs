using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.Post;

public class Slug : ValueObject
{
    public const int MaxLength = 1024;
    private Slug()
    {
    }

    public Slug(string value) => Value = value;


    public string Value { get; private set; } = null!;

    public static Result<Slug> Create(string slug) =>
        Result.Create(slug)
            .Ensure(s => !string.IsNullOrEmpty(s),
                DomainErrors.Slug.Empty)
            .Ensure(s => s.Length <= MaxLength,
                DomainErrors.Slug.TooLong)
            .Map(s => new Slug(s));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}