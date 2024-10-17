// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class ContestModelBuilder :
    IEntityTypeConfiguration<Contest>,
    IEntityTypeConfiguration<ContestTranslation>
{
    public void Configure(EntityTypeBuilder<Contest> builder)
    {
        builder
            .HasMany(x => x.Translations)
            .WithOne(x => x.Contest!)
            .HasForeignKey(x => x.ContestId);

        builder
            .Property(d => d.Date)
            .HasDateType()
            .HasUtcConversion();

        builder
            .HasOne(x => x.DomainOfInfluence!)
            .WithOne(x => x.ManagedContest!)
            .HasForeignKey<Contest>(x => x.DomainOfInfluenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.ContestDomainOfInfluences)
            .WithOne(x => x.Contest!)
            .IsRequired();

        builder
            .HasMany(x => x.ContestCountingCircles)
            .WithOne(x => x.Contest!)
            .IsRequired();

        builder.Property(x => x.Approved)
            .HasUtcConversion();

        builder.Property(x => x.AttachmentDeliveryDeadline)
            .HasUtcConversion();

        builder.Property(x => x.PrintingCenterSignUpDeadline)
            .HasUtcConversion();

        builder.Property(x => x.GenerateVotingCardsDeadline)
            .HasUtcConversion();

        builder.HasIndex(x => x.State);
    }

    public void Configure(EntityTypeBuilder<ContestTranslation> builder)
    {
        builder.HasLanguageQueryFilter();

        builder
            .HasIndex(b => new { b.ContestId, b.Language })
            .IsUnique();
    }
}
