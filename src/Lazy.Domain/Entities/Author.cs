using Lazy.Domain.Primitives;

namespace Lazy.Domain.Entities;

public class Author : AggregateRoot, IAuditableEntity
{


    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}