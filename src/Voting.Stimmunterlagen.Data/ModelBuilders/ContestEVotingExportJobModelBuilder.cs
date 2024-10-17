// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class ContestEVotingExportJobModelBuilder : IEntityTypeConfiguration<ContestEVotingExportJob>
{
    public void Configure(EntityTypeBuilder<ContestEVotingExportJob> builder)
    {
        builder
            .HasOne(x => x.Contest)
            .WithOne(x => x.EVotingExportJob)
            .HasForeignKey<ContestEVotingExportJob>(x => x.ContestId)
            .IsRequired();

        builder.Property(x => x.Started).HasUtcConversion();
        builder.Property(x => x.Failed).HasUtcConversion();
        builder.Property(x => x.Completed).HasUtcConversion();
    }
}
