using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.Arcade;

public class GameKey : ValueObject
{
    public const int MaxLength = 50;

    public static readonly GameKey Snake = new("snake");

    private GameKey()
    {
    }

    private GameKey(string value) => Value = value;

    public string Value { get; private set; } = null!;

    public static Result<GameKey> Create(string gameKey) =>
        Result.Create(gameKey)
            .Ensure(
                g => !string.IsNullOrWhiteSpace(g),
                DomainErrors.ArcadeScore.GameKeyEmpty)
            .Ensure(
                g => g.Length <= MaxLength,
                DomainErrors.ArcadeScore.GameKeyTooLong)
            .Map(g => new GameKey(g.Trim().ToLowerInvariant()));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
