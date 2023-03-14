using Lazy.Domain.DomainEvents;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities;

public sealed class User : IdentityUser<Guid>, IAuditableEntity
{
    private readonly List<Post> _posts = new();
    private readonly List<Comment> _comments = new();
    private readonly List<UserClaim> _claims = new();
    private readonly List<UserLogin> _logins = new ();
    private readonly List<UserToken> _tokens = new();
    private readonly List<UserRole> _userRoles = new();

    private User(Guid id, Email email, FirstName firstName, LastName lastName, string userName)
        : base(email.Value)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
    }

    private User()
    {
    }

    public new Email Email { get; private set; }

    public FirstName FirstName { get; set; }

    public LastName LastName { get; set; }

    public IReadOnlyCollection<Post> Posts => _posts;
    public IReadOnlyCollection<Comment> Comments => _comments;

    public IReadOnlyCollection<UserClaim> Claims => _claims;

    public IReadOnlyCollection<UserLogin> Logins => _logins;

    public IReadOnlyCollection<UserToken> Tokens => _tokens;

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }


    public static User Create(
        Guid id,
        Email email,
        FirstName firstName,
        LastName lastName)
    {
        string userName = email.Value.Split('@').First();

        var user = new User(
            id,
            email, 
            firstName, 
            lastName,
            userName);

        return user;
    }

    public void ChangeName(FirstName firstName, LastName lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}