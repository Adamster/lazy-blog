using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public class Tag : Entity, IAuditableEntity
{
    private readonly List<Post> _posts = new();

    public const int MaxLength = 50;

    public string Value { get; private set; }

    public IReadOnlyCollection<Post> Posts => _posts;

    private Tag()
    {
    }

    private Tag(string value) => Value = value;

    public static Result<Tag> Create(string tag) =>
        Result.Create(tag)
            .Ensure(
                e => !string.IsNullOrEmpty(e),
                DomainErrors.Tag.Empty)
            .Ensure(e => e.Length <= MaxLength,
                DomainErrors.Tag.TooLong)
            .Map(t => new Tag(t));

    public void Update(string tag)
    {
        var newTag = Create(tag);
        if (newTag.IsSuccess)
        {
            Value = tag;
        }
    }

    public void Update(Tag tag)
    {
        Value = tag.Value;
    }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}