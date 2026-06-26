using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User;

public sealed class DisplayName : ValueObject
{
    public const int MaxLength = 100;

    private DisplayName(string value)
    {
        Value = value;
    }

    private DisplayName()
    {
    }

    public string Value { get; private set; } = null!;

    public static Result<DisplayName> Create(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<DisplayName>(DomainErrors.DisplayName.Empty);
        }

        var trimmed = displayName.Trim();

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<DisplayName>(DomainErrors.DisplayName.TooLong);
        }

        return new DisplayName(trimmed);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public void Update(DisplayName displayName) => Value = displayName.Value;
}
