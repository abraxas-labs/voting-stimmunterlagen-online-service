// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class VotingCardPrintFileExportJobModelBuilder : IEntityTypeConfiguration<VotingCardPrintFileExportJob>
{
    public void Configure(EntityTypeBuilder<VotingCardPrintFileExportJob> builder)
    {
        builder
            .HasOne(x => x.VotingCardGeneratorJob)
            .WithOne(x => x.VotingCardPrintFileExportJob)
            .HasForeignKey<VotingCardPrintFileExportJob>(x => x.VotingCardGeneratorJobId)
            .IsRequired();

        builder.Property(x => x.Started).HasUtcConversion();
        builder.Property(x => x.Failed).HasUtcConversion();
        builder.Property(x => x.Completed).HasUtcConversion();
    }
}
