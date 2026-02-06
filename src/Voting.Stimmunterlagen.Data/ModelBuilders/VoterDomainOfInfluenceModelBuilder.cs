// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

internal class VoterDomainOfInfluenceModelBuilder : IEntityTypeConfiguration<VoterDomainOfInfluence>
{
    public void Configure(EntityTypeBuilder<VoterDomainOfInfluence> builder)
    {
        builder
            .HasOne(x => x.Voter)
            .WithMany(x => x!.DomainOfInfluences)
            .HasForeignKey(x => x.VoterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
