using Lazy.Domain.DomainEvents;
using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects;

namespace Lazy.Domain.Entities;

public class Author : AggregateRoot, IAuditableEntity
{
    private Author(Guid id, Email email, FirstName firstName, LastName lastName)
        : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
    private Author()
    {
    }

    public Email Email { get; private set; }
    public FirstName FirstName { get; private set; }
    public LastName LastName { get; private set; }
    
    public DateTime CreatedOnUtc { get; set; }
    
    public DateTime? UpdatedOnUtc { get; set; }

    public static Author Create(
        Guid id,
        Email email,
        FirstName firstName,
        LastName lastName)
    {
        var author = new Author(
            id,
            email, 
            firstName, 
            lastName);
        
        author.RaiseDomainEvent(new AuthorRegisteredDomainEvent(
                Guid.NewGuid(),
                author.Id));

        return author;
    }
}