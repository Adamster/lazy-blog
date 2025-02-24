using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.Post;

public class Body : ValueObject
{
    private Body()
    {
    }

    public Body(string value) => Value = value;

    public string Value { get; private set; } = null!;

    public static Result<Body> Create(string body) =>
        Result.Create(body)
            .Ensure(b => !string.IsNullOrEmpty(b),
                DomainErrors.Body.Empty)
            .Map(b => new Body(b));


    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}