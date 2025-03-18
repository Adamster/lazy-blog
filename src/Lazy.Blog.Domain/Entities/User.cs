using Lazy.Domain.DomainEvents;
using Lazy.Domain.Entities.Identity;
using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects.User;
using Microsoft.AspNetCore.Identity;
// ReSharper disable CollectionNeverUpdated.Local

namespace Lazy.Domain.Entities;

public sealed class User : IdentityUser<Guid>, IAuditableEntity
{
    private readonly List<Post> _posts = new();
    private readonly List<Comment> _comments = new();
    private readonly List<UserClaim> _claims = new();
    private readonly List<UserLogin> _logins = new();
    private readonly List<UserToken> _tokens = new();
    private readonly List<UserRole> _userRoles = new();
    private readonly List<PostVote> _postVotes = new();

    private User(Guid id, Email email, FirstName firstName, LastName lastName, UserName userName, Biography? biography)
        : base(email.Value)
    {
        Id = id;
        Email = email.Value;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName.Value;
        Biography = biography;
        CreatedOnUtc = DateTime.UtcNow;
    }

    private User()
    {
    }
    
    public FirstName FirstName { get; init; } = null!;

    public LastName LastName { get; init; } = null!;

    public Avatar? Avatar { get; private set; }

    public Biography? Biography { get; init; }

    public IReadOnlyCollection<Post> Posts => _posts;
    public IReadOnlyCollection<Comment> Comments => _comments;

    public IReadOnlyCollection<PostVote> PostVotes => _postVotes;

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
        LastName lastName,
        UserName userName,
        Biography? biography)
    {
        var user = new User(
            id,
            email,
            firstName,
            lastName,
            userName, 
            biography);

        return user;
    }

    public void UpdateUser(FirstName firstName, LastName lastName, UserName userName, Biography? biography)
    {
        FirstName.Update(firstName);
        LastName.Update(lastName);
        Biography?.Update(biography);
        UserName = userName.Value;
    }

    public void SetAvatar(Avatar avatar)
    {
        Avatar = avatar;
    }
}