// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class VotingCardGeneratorJobModelBuilder : IEntityTypeConfiguration<VotingCardGeneratorJob>
{
    public void Configure(EntityTypeBuilder<VotingCardGeneratorJob> builder)
    {
        builder
            .HasOne(x => x.Layout)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.LayoutId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x.VotingCardGeneratorJobs)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder
            .HasMany(x => x.Voter)
            .WithOne(x => x.Job!)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Started).HasUtcConversion();
        builder.Property(x => x.Failed).HasUtcConversion();
        builder.Property(x => x.Completed).HasUtcConversion();
    }
}
