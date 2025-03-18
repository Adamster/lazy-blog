using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User;

public sealed class Biography : ValueObject
{
    public const int MaxLength = 4000;

    public string? Value { get; private set; }

    private Biography(string value)
    {
        Value = value;
    }

    private Biography()
    {
    }

    public static Result<Biography> Create(string biography)
    {
        if (biography.Length > MaxLength)
        {
            return Result.Failure<Biography>(DomainErrors.Biography.TooLong);
        }

        return new Biography(biography);
    }


    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value ?? string.Empty;
    }

    public void Update(Biography? biography) => Value = biography?.Value;
}