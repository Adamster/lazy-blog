using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User;

public class UserName : ValueObject
{
    public const int MaxLength = 255;

    private UserName(string value)
    {
        Value = value;
    }

    private UserName()
    {
    }

    public string Value { get; private set; } = null!;


    public static Result<UserName> Create(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return Result.Failure<UserName>(DomainErrors.UserName.Empty);
        }

        if (userName.Length > MaxLength)
        {
            return Result.Failure<UserName>(DomainErrors.UserName.TooLong);
        }

        return new UserName(userName);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}