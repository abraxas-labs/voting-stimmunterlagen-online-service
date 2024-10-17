// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class VoterListModelBuilder : IEntityTypeConfiguration<VoterList>,
    IEntityTypeConfiguration<PoliticalBusinessVoterListEntry>
{
    public void Configure(EntityTypeBuilder<VoterList> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x!.VoterLists)
            .IsRequired();

        builder
            .HasIndex(x => new { x.ImportId, x.VotingCardType })
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<PoliticalBusinessVoterListEntry> builder)
    {
        builder.HasIndex(x => new { x.PoliticalBusinessId, x.VoterListId })
            .IsUnique();

        builder.HasOne(x => x.PoliticalBusiness!)
            .WithMany(x => x.VoterListEntries!)
            .HasForeignKey(x => x.PoliticalBusinessId)
            .IsRequired();

        builder.HasOne(x => x.VoterList!)
            .WithMany(x => x.PoliticalBusinessEntries!)
            .HasForeignKey(x => x.VoterListId)
            .IsRequired();
    }
}
