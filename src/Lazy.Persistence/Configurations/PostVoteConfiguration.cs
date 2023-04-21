using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lazy.Persistence.Configurations;

public class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
{
    public void Configure(EntityTypeBuilder<PostVote> builder)
    {
        builder.ToTable(TableNames.PostVotes);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.VoteDirection)
            .HasConversion(new EnumToStringConverter<VoteDirection>());
    }
}