using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.Post;

public class Title : ValueObject
{
    public const int MaxLength = 100;

    private Title()
    {
    }

    private Title(string value) => Value = value;

    public string Value { get; private set; } = null!;

    public static Result<Title> Create(string title) =>
        Result.Create(title)
            .Ensure(
                t => !string.IsNullOrEmpty(t),
                DomainErrors.Title.Empty)
            .Ensure(t => t.Length <= MaxLength,
                DomainErrors.Title.TooLong)
            .Map(t => new Title(t));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}