﻿using Lazy.Domain.Primitives;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public sealed class UserToken : IdentityUserToken<Guid>, IAuditableEntity
{
    private const int RefreshTokenLifeTimeInMonths = 6;
    private const string LazyBlogToken = nameof(LazyBlogToken);
    private const string LazyProviderName = "lazy-credentials";

    public UserToken(string tokenId, User user)
    {
        Value = Guid.NewGuid().ToString();
        JwtId = tokenId;
        UserId = user.Id;
        CreatedOnUtc = DateTime.UtcNow;
        ExpiryDate = DateTime.UtcNow.AddMonths(RefreshTokenLifeTimeInMonths);
        Name = LazyBlogToken;
        LoginProvider = LazyProviderName;
    }

    public UserToken()
    {
    }

    public User User { get; set; } = null!;

    public DateTime ExpiryDate { get; private set; }

    public bool IsUsed { get; private set; }

    public bool IsInvalidated { get; private set; }

    public string JwtId { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public void UseToken() => IsUsed = true;

    public void Invalidate() => IsInvalidated = true;
}