// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class ContestDomainOfInfluenceModelBuilder : IEntityTypeConfiguration<ContestDomainOfInfluence>,
    IEntityTypeConfiguration<ContestDomainOfInfluenceHierarchyEntry>,
    IEntityTypeConfiguration<ContestDomainOfInfluenceCountingCircle>
{
    public void Configure(EntityTypeBuilder<ContestDomainOfInfluence> builder)
    {
        builder
            .HasOne(di => di.Parent!)
            .WithMany(di => di.Children)
            .HasForeignKey(di => di.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.GenerateVotingCardsTriggered)
            .HasUtcConversion();

        builder.HasOne(x => x.Root)
            .WithMany(x => x.RootOfChildrenAndSelf)
            .HasForeignKey(x => x.RootId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasGinTrigramIndex(x => x.AuthorityName);
        builder.HasGinTrigramIndex(x => x.Name);

        builder
            .Property(d => d.LastVoterUpdate)
            .HasUtcConversion();

        builder.OwnsOne(doi => doi.ReturnAddress);
        builder.OwnsOne(doi => doi.PrintData);
        builder.OwnsOne(doi => doi.SwissPostData);
        builder.OwnsOne(doi => doi.CantonDefaults);
    }

    public void Configure(EntityTypeBuilder<ContestDomainOfInfluenceHierarchyEntry> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x!.HierarchyEntries)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder
            .HasOne(x => x.ParentDomainOfInfluence)
            .WithMany(x => x!.ParentHierarchyEntries)
            .HasForeignKey(x => x.ParentDomainOfInfluenceId)
            .IsRequired();

        builder.HasIndex(x => new { x.DomainOfInfluenceId, x.ParentDomainOfInfluenceId })
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<ContestDomainOfInfluenceCountingCircle> builder)
    {
        builder
            .HasOne(dicc => dicc.CountingCircle)
            .WithMany(cc => cc.DomainOfInfluences)
            .HasForeignKey(dicc => dicc.CountingCircleId)
            .IsRequired();

        builder
            .HasOne(dicc => dicc.DomainOfInfluence)
            .WithMany(di => di.CountingCircles)
            .HasForeignKey(dicc => dicc.DomainOfInfluenceId)
            .IsRequired();

        builder
            .HasIndex(x => new { x.CountingCircleId, x.DomainOfInfluenceId })
            .IsUnique();
    }
}
