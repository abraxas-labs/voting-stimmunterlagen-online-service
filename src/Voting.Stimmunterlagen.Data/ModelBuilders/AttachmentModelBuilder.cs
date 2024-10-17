// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Extensions;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class AttachmentModelBuilder : IEntityTypeConfiguration<Attachment>,
    IEntityTypeConfiguration<PoliticalBusinessAttachmentEntry>,
    IEntityTypeConfiguration<DomainOfInfluenceAttachmentCount>,
    IEntityTypeConfiguration<AttachmentComment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder
            .Property(x => x.DeliveryPlannedOn)
            .HasDateType()
            .HasUtcConversion();

        builder
            .Property(x => x.DeliveryReceivedOn)
            .HasDateType()
            .HasUtcConversion();

        builder
            .HasOne(x => x.DomainOfInfluence)
            .WithMany(x => x.Attachments)
            .IsRequired();

        builder
            .HasGinTrigramIndex(x => x.Name);
    }

    public void Configure(EntityTypeBuilder<PoliticalBusinessAttachmentEntry> builder)
    {
        builder.HasIndex(x => new { x.PoliticalBusinessId, x.AttachmentId })
            .IsUnique();

        builder.HasOne(x => x.PoliticalBusiness!)
            .WithMany(x => x.AttachmentEntries!)
            .HasForeignKey(x => x.PoliticalBusinessId)
            .IsRequired();

        builder.HasOne(x => x.Attachment!)
            .WithMany(x => x.PoliticalBusinessEntries!)
            .HasForeignKey(x => x.AttachmentId)
            .IsRequired();
    }

    public void Configure(EntityTypeBuilder<DomainOfInfluenceAttachmentCount> builder)
    {
        builder.HasIndex(x => new { x.DomainOfInfluenceId, x.AttachmentId })
            .IsUnique();

        builder.HasOne(x => x.DomainOfInfluence!)
            .WithMany(x => x.DomainOfInfluenceAttachmentCounts!)
            .HasForeignKey(x => x.DomainOfInfluenceId)
            .IsRequired();

        builder.HasOne(x => x.Attachment!)
            .WithMany(x => x.DomainOfInfluenceAttachmentCounts)
            .HasForeignKey(x => x.AttachmentId)
            .IsRequired();
    }

    public void Configure(EntityTypeBuilder<AttachmentComment> builder)
    {
        builder
            .HasOne(x => x.Attachment)
            .WithMany(x => x!.Comments)
            .IsRequired();

        builder
            .OwnsOne(
                x => x.CreatedBy,
                u =>
                {
                    u.Property(uu => uu.SecureConnectId).IsRequired();
                    u.Property(uu => uu.FirstName).IsRequired();
                    u.Property(uu => uu.LastName).IsRequired();
                });

        builder
            .Property(x => x.CreatedAt)
            .HasUtcConversion();
    }
}
