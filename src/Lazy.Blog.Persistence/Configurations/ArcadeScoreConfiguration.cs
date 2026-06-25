using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Arcade;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

internal sealed class ArcadeScoreConfiguration : IEntityTypeConfiguration<ArcadeScore>
{
    public void Configure(EntityTypeBuilder<ArcadeScore> builder)
    {
        builder.ToTable(TableNames.ArcadeScores);

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Game)
            .HasConversion(g => g.Value, v => GameKey.Create(v).Value)
            .HasMaxLength(GameKey.MaxLength);

        builder.Property(s => s.Score);

        builder
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.Game, s.UserId, s.Score });
    }
}
