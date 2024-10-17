// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class TemplateDataFieldModelBuilder : IEntityTypeConfiguration<TemplateDataField>
{
    public void Configure(EntityTypeBuilder<TemplateDataField> builder)
    {
        builder
            .HasOne(x => x.Container!)
            .WithMany(x => x.Fields)
            .HasForeignKey(x => x.ContainerId)
            .IsRequired();

        builder
            .HasIndex(x => new { x.ContainerId, x.Key })
            .IsUnique();
    }
}
