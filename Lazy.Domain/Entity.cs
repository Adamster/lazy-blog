namespace Lazy.Domain;

public abstract class Entity
{
    protected Entity()
    {
        CreatedAt = DateTime.Now;
    }

    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}