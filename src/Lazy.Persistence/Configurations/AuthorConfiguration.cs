﻿using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable(TableNames.Authors);

        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Email)
            .HasConversion(x => x.Value, v => Email.Create(v).Value);

        builder
            .Property(x => x.FirstName)
            .HasConversion(x => x.Value, v => FirstName.Create(v).Value)
            .HasMaxLength(FirstName.MaxLength);

        builder
            .Property(x => x.LastName)
            .HasConversion(x => x.Value, v => LastName.Create(v).Value)
            .HasMaxLength(LastName.MaxLength);
        
        builder.HasIndex(x => x.Email).IsUnique();
    }
}