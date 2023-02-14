using Lazy.Domain.DomainEvents;
using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects;

namespace Lazy.Domain.Entities;

public class User : AggregateRoot, IAuditableEntity
{
    private readonly List<Post> _posts = new();
    private readonly List<Comment> _comments = new();

    private User(Guid id, Email email, FirstName firstName, LastName lastName)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    private User()
    {
    }

    public Email Email { get; private set; }

    public FirstName FirstName { get; set; }

    public LastName LastName { get; set; }

    public IReadOnlyCollection<Post> Posts => _posts;
    public IReadOnlyCollection<Comment> Comments => _comments;

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }


    public static User Create(
        Guid id,
        Email email,
        FirstName firstName,
        LastName lastName)
    {
        var user = new User(
            id,
            email, 
            firstName, 
            lastName);
        
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(
            Guid.NewGuid(),
            user.Id));

        return user;
    }
}