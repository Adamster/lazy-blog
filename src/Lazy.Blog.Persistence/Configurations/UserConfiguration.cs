using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.User;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNames.Users);

        builder.HasKey(x => x.Id);

        builder.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
        builder.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");

        builder.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

        builder.Property(u => u.UserName).HasMaxLength(Email.MaxLength);
        builder.Property(u => u.NormalizedUserName).HasMaxLength(Email.MaxLength);
        builder.Property(u => u.NormalizedEmail).HasMaxLength(Email.MaxLength);


        builder
            .Property(x => x.Email)
            .HasConversion(x => x.Value, v => Email.Create(v).Value)
            .HasMaxLength(Email.MaxLength);

        builder
            .Property(x => x.FirstName)
            .HasConversion(x => x.Value, v => FirstName.Create(v).Value)
            .HasMaxLength(FirstName.MaxLength);

        builder
            .Property(x => x.LastName)
            .HasConversion(x => x.Value, v => LastName.Create(v).Value)
            .HasMaxLength(LastName.MaxLength);

        builder.Property(x => x.UserName)
            .HasConversion(x => x.Value, v => UserName.Create(v).Value)
            .HasMaxLength(UserName.MaxLength);

        builder.OwnsOne(p => p.Avatar, avatarBuilder =>
        {
            avatarBuilder.Property(x => x.Url).HasMaxLength(Avatar.MaxUrlLength);
            avatarBuilder.Property(x => x.Filename).HasMaxLength(Avatar.MaxFilenameLength);
        });

        builder
            .HasMany(u => u.Posts)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        builder
            .HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.PostVotes)
            .WithOne(pv => pv.User)
            .HasForeignKey(pv => pv.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(e => e.Claims)
            .WithOne(c => c.User)
            .HasForeignKey(uc => uc.UserId)
            .IsRequired();

        // Each User can have many UserLogins
        builder
            .HasMany(e => e.Logins)
            .WithOne(l => l.User)
            .HasForeignKey(ul => ul.UserId)
            .IsRequired();

        // Each User can have many UserTokens
        builder
            .HasMany(e => e.Tokens)
            .WithOne(t => t.User)
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        // Each User can have many entries in the UserRole join table
        builder
            .HasMany(e => e.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.UserName).IsUnique();

    }
}