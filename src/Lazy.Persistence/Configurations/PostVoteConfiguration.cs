using Lazy.Domain.Entities;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

public class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
{
    public void Configure(EntityTypeBuilder<PostVote> builder)
    {
        builder.ToTable(TableNames.PostVotes);

        builder.HasKey(p => new
        {
            p.UserId, p.PostId
        });

        builder.Property(p => p.VoteDirection);
    }
}