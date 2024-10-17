// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class DomainOfInfluenceVotingCardConfigurationModelBuilder
    : IEntityTypeConfiguration<DomainOfInfluenceVotingCardConfiguration>
{
    public void Configure(EntityTypeBuilder<DomainOfInfluenceVotingCardConfiguration> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence!)
            .WithOne(x => x.VotingCardConfiguration!)
            .HasForeignKey<DomainOfInfluenceVotingCardConfiguration>(x => x.DomainOfInfluenceId)
            .IsRequired();
    }
}
