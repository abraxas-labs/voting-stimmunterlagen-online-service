// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class PoliticalBusinessModelBuilder :
    IEntityTypeConfiguration<PoliticalBusiness>,
    IEntityTypeConfiguration<PoliticalBusinessTranslation>,
    IEntityTypeConfiguration<PoliticalBusinessPermissionEntry>
{
    public void Configure(EntityTypeBuilder<PoliticalBusiness> builder)
    {
        builder
            .HasMany(x => x.Translations)
            .WithOne(x => x.PoliticalBusiness!)
            .HasForeignKey(x => x.PoliticalBusinessId);

        builder
            .HasOne(x => x.Contest!)
            .WithMany(x => x.PoliticalBusinesses)
            .HasForeignKey(x => x.ContestId)
            .IsRequired();

        builder
            .HasOne(x => x.DomainOfInfluence!)
            .WithMany(x => x.PoliticalBusinesses)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder
            .HasMany(x => x.PermissionEntries!)
            .WithOne(x => x.PoliticalBusiness!)
            .HasForeignKey(x => x.PoliticalBusinessId)
            .IsRequired();
    }

    public void Configure(EntityTypeBuilder<PoliticalBusinessTranslation> builder)
    {
        builder.HasLanguageQueryFilter();

        builder
            .HasIndex(b => new { b.PoliticalBusinessId, b.Language })
            .IsUnique();
    }

    public void Configure(EntityTypeBuilder<PoliticalBusinessPermissionEntry> builder)
    {
        builder.HasIndex(x => new { x.PoliticalBusinessId, x.DomainOfInfluenceId, x.Role })
            .IsUnique();

        builder.HasOne(x => x.PoliticalBusiness!)
            .WithMany(x => x.PermissionEntries!)
            .HasForeignKey(x => x.PoliticalBusinessId)
            .IsRequired();

        builder.HasOne(x => x.DomainOfInfluence!)
            .WithMany(x => x.PoliticalBusinessPermissionEntries!)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();
    }
}
