using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User
{
    public sealed class Email : ValueObject
    {
        public const int MaxLength = 255;

        private Email()
        {
        }

        private Email(string value) => Value = value;

        public string Value { get; private set; }

        public static Result<Email> Create(string email) =>
            Result.Create(email)
                .Ensure(
                    e => !string.IsNullOrEmpty(e),
                    DomainErrors.Email.Empty)
                .Ensure(e => e.Length <= MaxLength,
                    DomainErrors.Email.TooLong)
                .Ensure(e => e.Split('@').Length == 2,
                    DomainErrors.Email.InvalidFormat)
                .Map(e => new Email(e));

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}
