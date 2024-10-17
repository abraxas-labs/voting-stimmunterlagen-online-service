// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class ManualVotingCardGeneratorJobModelBuilder : IEntityTypeConfiguration<ManualVotingCardGeneratorJob>
{
    public void Configure(EntityTypeBuilder<ManualVotingCardGeneratorJob> builder)
    {
        builder.Property(x => x.Created).HasUtcConversion();
        builder.OwnsOne(x => x.CreatedBy);

        builder
            .HasOne(x => x.Layout)
            .WithMany(x => x.ManualJobs)
            .HasForeignKey(x => x.LayoutId)
            .IsRequired();

        builder
            .HasOne(x => x.Voter)
            .WithOne(x => x.ManualJob!)
            .HasForeignKey<Voter>(x => x.ManualJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
