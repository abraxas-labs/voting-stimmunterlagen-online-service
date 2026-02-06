// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class VoterModelBuilder : IEntityTypeConfiguration<Voter>,
    IEntityTypeConfiguration<DomainOfInfluenceVoterDuplicate>
{
    public void Configure(EntityTypeBuilder<Voter> builder)
    {
        builder.OwnsOne(x => x.Country);
        builder.OwnsOne(x => x.PageInfo);
        builder.OwnsMany(x => x.PlacesOfOrigin);
        builder.OwnsOne(x => x.SwissAbroadPerson, x =>
        {
            x.OwnsOne(x => x.ResidenceCountry);
            x.Property(x => x.DateOfRegistration)
                .HasDateType()
                .HasUtcConversion();

            x.OwnsOne(x => x.Extension, y =>
            {
                y.OwnsOne(z => z.Authority, a =>
                {
                    a.OwnsOne(b => b.Organisation);
                    a.OwnsOne(b => b.Country);
                });
                y.OwnsOne(z => z.Address);
            });
        });

        builder
            .HasOne(x => x.List)
            .WithMany(x => x!.Voters)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Contest)
            .WithMany(x => x.Voters)
            .HasForeignKey(x => x.ContestId)
            .IsRequired();

        builder
            .HasOne(x => x.VoterDuplicate)
            .WithMany(x => x.Voters)
            .HasForeignKey(x => x.VoterDuplicateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasIndex(x => new { x.ContestId, x.ContestIndex })
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<DomainOfInfluenceVoterDuplicate> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x.VoterDuplicates)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();
    }
}
