// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Data.ModelBuilders;

public class TemplateDataFieldValueModelBuilder : IEntityTypeConfiguration<TemplateDataFieldValue>
{
    public void Configure(EntityTypeBuilder<TemplateDataFieldValue> builder)
    {
        builder
            .HasOne(x => x.Field!)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.FieldId)
            .IsRequired();

        builder
            .HasOne(x => x.Layout!)
            .WithMany(x => x.TemplateDataFieldValues)
            .HasForeignKey(x => x.LayoutId)
            .IsRequired();

        builder
            .HasIndex(x => new { x.FieldId, x.LayoutId })
            .IsUnique();
    }
}
