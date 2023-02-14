using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.Post;

public class Tag : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; private set; }

    private Tag()
    {
    }

    private Tag(string value) => Value = value;

    public static Result<Tag> Create(string tag) =>
        Result.Create(tag)
            .Ensure(
                e => !string.IsNullOrEmpty(e),
                DomainErrors.Tag.Empty)
            .Ensure(e => e.Length <= MaxLength,
                DomainErrors.Tag.TooLong)
            .Map(t => new Tag(t));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}