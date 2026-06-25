using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Domain.Entities;

public sealed class ArcadeScore : Entity, IAuditableEntity
{
    private ArcadeScore(Guid id, Guid userId, GameKey game, int score)
        : base(id)
    {
        UserId = userId;
        Game = game;
        Score = score;
    }

    private ArcadeScore()
    {
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public GameKey Game { get; private set; } = null!;

    public int Score { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public static ArcadeScore Create(Guid userId, GameKey game, int score) =>
        new(Guid.NewGuid(), userId, game, score);
}
