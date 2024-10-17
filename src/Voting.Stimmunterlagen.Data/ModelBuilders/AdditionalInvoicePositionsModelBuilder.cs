// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class AdditionalInvoicePositionsModelBuilder : IEntityTypeConfiguration<AdditionalInvoicePosition>
{
    public void Configure(EntityTypeBuilder<AdditionalInvoicePosition> builder)
    {
        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x.AdditionalInvoicePositions)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder.Property(x => x.Created).HasUtcConversion();
        builder.Property(x => x.Modified).HasUtcConversion();

        builder.OwnsOne(
            x => x.CreatedBy,
            u =>
            {
                u.Property(uu => uu.SecureConnectId).IsRequired();
                u.Property(uu => uu.FirstName).IsRequired();
                u.Property(uu => uu.LastName).IsRequired();
            });

        builder.OwnsOne(
            x => x.ModifiedBy,
            u =>
            {
                u.Property(uu => uu.SecureConnectId).IsRequired();
                u.Property(uu => uu.FirstName).IsRequired();
                u.Property(uu => uu.LastName).IsRequired();
            });
    }
}
