﻿using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable(TableNames.Posts);

        builder.HasKey(p => p.Id);

        builder.Property(x => x.Title)
            .HasConversion(x => x.Value, v => Title.Create(v).Value)
            .HasMaxLength(Title.MaxLength);

        builder.Property(x => x.Summary)
            .HasConversion(x => x != null ? x.Value : string.Empty, v => Summary.Create(v).Value)
            .HasMaxLength(Summary.MaxLength);

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, v => Slug.Create(v).Value)
            .HasMaxLength(Slug.MaxLength)
            .HasDefaultValueSql("NEWID()");

        builder.Property(x => x.Body)
            .HasConversion(x => x.Value, v => Body.Create(v).Value);

        builder
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts);

        builder.Property(x => x.CoverUrl);
        builder.Property(x => x.Views);
        builder.Property(x => x.IsPublished);
        builder.Property(x => x.Rating);
        builder.Property(x => x.IsCoverDisplayed);

        builder.Property(x => x.PublishedOnUtc);
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}