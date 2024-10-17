// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class VotingCardLayoutModelBuilder
    : IEntityTypeConfiguration<ContestVotingCardLayout>, IEntityTypeConfiguration<DomainOfInfluenceVotingCardLayout>
{
    public void Configure(EntityTypeBuilder<ContestVotingCardLayout> builder)
    {
        builder
            .HasOne(x => x.Contest!)
            .WithMany(x => x.VotingCardLayouts)
            .HasForeignKey(x => x.ContestId)
            .IsRequired();

        builder
            .HasIndex(x => new { x.ContestId, x.VotingCardType })
            .IsUnique();

        builder
            .HasOne(x => x.Template!)
            .WithMany(x => x.ContestVotingCardLayouts)
            .HasForeignKey(x => x.TemplateId);
    }

    public void Configure(EntityTypeBuilder<DomainOfInfluenceVotingCardLayout> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence!)
            .WithMany(x => x.VotingCardLayouts)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder
            .HasIndex(x => new { x.DomainOfInfluenceId, x.VotingCardType })
            .IsUnique();

        builder
            .HasOne(x => x.Template!)
            .WithMany(x => x.DomainOfInfluenceVotingCardLayouts)
            .HasForeignKey(x => x.TemplateId);

        builder
            .HasOne(x => x.OverriddenTemplate!)
            .WithMany(x => x.OverriddenDomainOfInfluenceVotingCardLayouts)
            .HasForeignKey(x => x.OverriddenTemplateId);
    }
}
