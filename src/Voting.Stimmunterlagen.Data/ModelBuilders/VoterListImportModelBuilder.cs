// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class VoterListImportModelBuilder : IEntityTypeConfiguration<VoterListImport>
{
    public void Configure(EntityTypeBuilder<VoterListImport> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x.VoterListImports)
            .IsRequired();

        builder
            .Property(x => x.LastUpdate)
            .HasDateType()
            .HasUtcConversion();
    }
}
